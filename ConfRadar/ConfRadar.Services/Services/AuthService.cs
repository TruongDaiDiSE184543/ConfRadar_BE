using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IAuthService
    {

        Task<int> RegisterAccount(CreateUserRequest request);
        Task VerifyRegistration(string token);
        Task<LoginUserResponse> LocalLogin(LocalLoginUserRequest request);
        Task<LoginUserResponse> FirebaseLogin(FirebaseLoginRequest request);
        Task ForgetPassword(string email);
        Task VerifyForgetPassword(string token, string newPassword);
        Task ChangePassword(string oldPassword, string newPassword, string userId);
        Task<LoginUserResponse> RefreshToken(string userId, string refreshToken);
    }
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ObjectStorageSettings _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly IFirebaseAuthService _firebaseAuthService;
        public AuthService(IPasswordHasher passwordHasher, IEmailService emailService, ITokenService tokenService, IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService, IOptions<ObjectStorageSettings> objectStorageSettings, IFirebaseAuthService firebaseAuthService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings.Value;
            _firebaseAuthService = firebaseAuthService;
        }
        public async Task<int> RegisterAccount(CreateUserRequest request)
        {
            request.Email = request.Email.Trim().ToLower();
            request.FullName = request.FullName.Trim();
            var userByEmail = await _unitOfWork.UserRepository.GetUserByEmail(request.Email);
            if (userByEmail != null)
            {
                throw new ConfRadarAuthenticationException("User with this email already exists");
            }
            var userByName = await _unitOfWork.UserRepository.GetUserByName(request.FullName);
            if (userByName != null)
            {
                throw new ConfRadarAuthenticationException("User with this full name already exists");
            }
            string fileUrl = null;
            if (request.AvatarFile != null)
            {
                if (request.AvatarFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                if (request.AvatarFile.ContentType != "image/png"
                    && request.AvatarFile.ContentType != "image/jpeg"
                    && request.AvatarFile.ContentType != "image/gif"
                    && request.AvatarFile.ContentType != "image/svg+xml"
                    && request.AvatarFile.ContentType != "image/webp")
                {
                    throw new BadRequestException("Only PNG,JPEG,GIF,SVG,WEBP files are allowed for avatar");
                }
                using var stream = request.AvatarFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.AvatarFile.FileName);
                var baseUri = _objectStorageSettings.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.avatar.ToString(), uniqueFileName, stream, request.AvatarFile.ContentType);
                fileUrl = baseUri + objectStorageFileUrl;
            }


            var hashedPassword = _passwordHasher.Hash(request.Password);
            var verificationToken = _tokenService.GenerateSecureRandomToken();
            string confirmationLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.ConfirmRegistrationEmail + $"?token={verificationToken}";
            var userCreated = UserMapper.FromCreateUserRequestToUser(request);
            userCreated.PasswordHash = hashedPassword;
            userCreated.VerificationToken = verificationToken;
            userCreated.LoginProvider = LoginProviderEnum.Local.ToString();
            userCreated.VerificationTokenExpiry = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(24), DateTimeKind.Unspecified);
            userCreated.AvatarUrl = fileUrl;
            var role = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Customer.GetDescription());
            var userRole = new UserRole()
            {
                UserId = userCreated.UserId,
                RoleId = role!.RoleId,
                AssignedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };
            userCreated.UserRoles.Add(userRole);
            await _emailService.SendAuthenticationTemplateEmailAsync(request.Email, request.FullName, confirmationLink, "Confirm Email Registration", "EmailRegistrationConfirmation.html");
            return await _unitOfWork.UserRepository.CreateUserAsync(userCreated);
        }
        public async Task VerifyRegistration(string token)
        {
            var user = await _unitOfWork.UserRepository.GetUserByRegistrationConfirmationToken(token);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException("Token not found");
            }
            if (user.IsEmailConfirmed == true)
            {
                throw new ConfRadarAuthenticationException("User is already confirmed");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user.VerificationTokenExpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            user.IsEmailConfirmed = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            await _unitOfWork.UserRepository.UpdateUserAsync(user);
        }
        public async Task<LoginUserResponse> LocalLogin(LocalLoginUserRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmail(request.Email);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException("User not found");
            }
            if (user.IsEmailConfirmed == false)
            {
                throw new ConfRadarAuthenticationException("Email is not confirmed");
            }
            if (user.IsActive == false)
            {
                throw new ConfRadarAuthenticationException("User is disabled");
            }
            if (!string.Equals(user.LoginProvider, LoginProviderEnum.Local.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfRadarAuthenticationException($"This account is registered with provider '{user.LoginProvider}'.");
            }
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new ConfRadarAuthenticationException("Invalid password");
            }

            var accessToken = await _tokenService.GenerateAccessToken(user.UserId, user.Email);
            var refreshToken = _tokenService.GenerateSecureRandomToken();
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            user.LastLogin = timeNow;
            UserRefreshToken userRefreshToken = new UserRefreshToken()
            {
                CreatedAt = timeNow,
                Expiry = timeNow.AddDays(_jwtSettings.ExpiresRefreshToken),
                IsRevoked = false,
                Token = refreshToken,
                UserId = user.UserId,
                TokenId = Guid.NewGuid().ToString(),
            };
            await _unitOfWork.UserRepository.UpdateUserAsync(user);
            await _unitOfWork.UserRefreshTokenRepository.CreateUserRefreshToken(userRefreshToken);
            return new LoginUserResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task ForgetPassword(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            var user = await _unitOfWork.UserRepository.GetUserByEmail(normalizedEmail);
            if (user == null)
            {
                throw new NotFoundException($"User with email {email} not found");
            }
            if (user.IsEmailConfirmed == false)
            {
                throw new ConfRadarAuthenticationException("Email is not confirmed");
            }
            var resetToken = _tokenService.GenerateSecureRandomToken();
            var resetLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.VerifyForgetPassword + $"?token={resetToken}";
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(60), DateTimeKind.Unspecified);
            await _unitOfWork.UserRepository.UpdateUserAsync(user);
            await _emailService.SendAuthenticationTemplateEmailAsync(email, user.FullName, resetLink, "Forget Password", "EmailForgetPassword.html");
        }

        public async Task VerifyForgetPassword(string token, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetUserByForgetPasswordToken(token);
            if (user == null)
            {
                throw new NotFoundException("Token is not found");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _unitOfWork.UserRepository.UpdateUserAsync(user);

        }

        public async Task ChangePassword(string oldPassword, string newPassword, string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException("User is not found");
            }
            if (!_passwordHasher.Verify(oldPassword, user.PasswordHash))
            {
                throw new ConfRadarAuthenticationException("Invalid old password");
            }
            user.PasswordHash = _passwordHasher.Hash(newPassword);
            await _unitOfWork.UserRepository.UpdateUserAsync(user);

        }

        public async Task<LoginUserResponse> FirebaseLogin(FirebaseLoginRequest request)
        {
            FirebaseToken? decodedToken = await _firebaseAuthService.VerifyIdTokenAsync(request.Token);
            if (decodedToken == null)
            {
                throw new ConfRadarAuthenticationException("Invalid Firebase token");
            }
            decodedToken.Claims.TryGetValue("email", out var emailFirebase);
            decodedToken.Claims.TryGetValue("name", out var nameFirebase);

            string? email = emailFirebase?.ToString();
            string? name = nameFirebase?.ToString();
            var user = await _unitOfWork.UserRepository.GetUserByEmail(email);
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user == null)
            {
                user = new User()
                {
                    Email = email,
                    FullName = name,
                    IsEmailConfirmed = true,
                    IsActive = true,
                    LastLogin = timeNow,
                    LoginProvider = LoginProviderEnum.Firebase.ToString(),
                    UserId = Guid.NewGuid().ToString(),
                    AvatarUrl = null,
                    CreatedAt = timeNow,
                };
                await _unitOfWork.UserRepository.CreateUserAsync(user);
            }
            else
            {
                if (!string.Equals(user.LoginProvider, LoginProviderEnum.Firebase.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ConfRadarAuthenticationException($"This account is registered with provider '{user.LoginProvider}'.");
                }
                user.LastLogin = timeNow;
                await _unitOfWork.UserRepository.UpdateUserAsync(user);
            }
            var accessToken = await _tokenService.GenerateAccessToken(user.UserId, user.Email);
            var refreshToken = _tokenService.GenerateSecureRandomToken();
            UserRefreshToken userRefreshToken = new UserRefreshToken()
            {
                CreatedAt = timeNow,
                IsRevoked = false,
                Token = refreshToken,
                TokenId = Guid.NewGuid().ToString(),
                UserId = user.UserId,
                Expiry = timeNow.AddMinutes(_jwtSettings.ExpiresRefreshToken),
            };
            await _unitOfWork.UserRefreshTokenRepository.CreateUserRefreshToken(userRefreshToken);
            return new LoginUserResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginUserResponse> RefreshToken(string userId, string refreshToken)
        {
            var tokenFound = await _unitOfWork.UserRefreshTokenRepository.GetUserRefreshTokenByRefreshToken(userId, refreshToken);
            if (tokenFound == null)
            {
                throw new ConfRadarAuthenticationException("Refresh token not found");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (timeNow >= tokenFound.Expiry)
            {
                throw new ConfRadarAuthenticationException("Refresh token is expired!");
            }
            if (tokenFound.IsRevoked == true)
            {
                throw new ConfRadarAuthenticationException("Refresh token is revoked!");
            }
            tokenFound.IsRevoked = true;
            await _unitOfWork.UserRefreshTokenRepository.UpdateUserRefreshToken(tokenFound);
            var accessToken = await _tokenService.GenerateAccessToken(tokenFound.UserId, tokenFound.User.Email!);
            var newRefreshToken = _tokenService.GenerateSecureRandomToken();
            UserRefreshToken userRefreshToken = new UserRefreshToken()
            {
                CreatedAt = timeNow,
                IsRevoked = false,
                Token = newRefreshToken,
                TokenId = Guid.NewGuid().ToString(),
                UserId = tokenFound.UserId,
                Expiry = timeNow.AddMinutes(_jwtSettings.ExpiresRefreshToken),
            };
            await _unitOfWork.UserRefreshTokenRepository.CreateUserRefreshToken(userRefreshToken);
            return new LoginUserResponse()
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}



