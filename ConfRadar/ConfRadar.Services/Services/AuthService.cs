using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IAuthService
    {

        Task<int> RegisterAccount(CreateUserRequest request);
        Task VerifyRegistration(string token);
        Task<LoginUserResponse> LocalLogin(LocalLoginUserRequest request);
        Task ForgetPassword(string email);
        Task VerifyForgetPassword(string token, string newPassword);
        Task ChangePassword(string oldPassword, string newPassword, string userId);
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
        public AuthService(IPasswordHasher passwordHasher, IEmailService emailService, ITokenService tokenService, IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService, IOptions<ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings.Value;
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
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucket.avatar.ToString(), uniqueFileName, stream, request.AvatarFile.ContentType);
                fileUrl = baseUri + objectStorageFileUrl;


            }

            var hashedPassword = _passwordHasher.Hash(request.Password);
            var verificationToken = _tokenService.GenerateSecureRandomToken();
            string confirmationLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.ConfirmRegistrationEmail + $"?token={verificationToken}";
            var userCreated = UserMapper.FromCreateUserRequestToUser(request);
            userCreated.Passwordhash = hashedPassword;
            userCreated.Verificationtoken = verificationToken;
            userCreated.Loginprovider = LoginProvider.Local.ToString();
            userCreated.Verificationtokenexpiry = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(24), DateTimeKind.Unspecified);
            userCreated.Avatarurl = fileUrl;
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
            if (user.Isemailconfirmed == true)
            {
                throw new ConfRadarAuthenticationException("User is already confirmed");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user.Verificationtokenexpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            user.Isemailconfirmed = true;
            user.Verificationtoken = null;
            user.Verificationtokenexpiry = null;
            await _unitOfWork.UserRepository.UpdateUserAsync(user);
        }
        public async Task<LoginUserResponse> LocalLogin(LocalLoginUserRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmail(request.Email);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException("User not found");
            }
            if (user.Isemailconfirmed == false)
            {
                throw new ConfRadarAuthenticationException("Email is not confirmed");
            }
            if (user.Isactive == false)
            {
                throw new ConfRadarAuthenticationException("User is disabled");
            }
            if (!_passwordHasher.Verify(request.Password, user.Passwordhash))
            {
                throw new ConfRadarAuthenticationException("Invalid password");
            }
            var accessToken = _tokenService.GenerateAccessToken(user.Userid, user.Email);
            var refreshToken = _tokenService.GenerateSecureRandomToken();
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            user.Lastlogin = timeNow;
            UserRefreshToken userRefreshToken = new UserRefreshToken()
            {
                Createdat = timeNow,
                Expiry = timeNow.AddMinutes(_jwtSettings.ExpiresRefreshToken),
                Isrevoked = false,
                Token = refreshToken,
                Userid = user.Userid,
                Tokenid = Guid.NewGuid().ToString(),
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
            if (user.Isemailconfirmed == false)
            {
                throw new ConfRadarAuthenticationException("Email is not confirmed");
            }
            var resetToken = _tokenService.GenerateSecureRandomToken();
            var resetLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.VerifyForgetPassword + $"?token={resetToken}";
            user.Passwordresettoken = resetToken;
            user.Passwordresettokenexpiry = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(60), DateTimeKind.Unspecified);
            await _unitOfWork.UserRepository.UpdateUserAsync(user);
            await _emailService.SendAuthenticationTemplateEmailAsync(email, user.Fullname, resetLink, "Forget Password", "EmailForgetPassword.html");
        }

        public async Task VerifyForgetPassword(string token, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetUserByForgetPasswordToken(token);
            if (user == null)
            {
                throw new NotFoundException("Token is not found");
            }
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (user.Passwordresettokenexpiry == null || user.Passwordresettokenexpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            user.Passwordhash = _passwordHasher.Hash(newPassword);
            user.Passwordresettoken = null;
            user.Passwordresettokenexpiry = null;
            await _unitOfWork.UserRepository.UpdateUserAsync(user);

        }

        public async Task ChangePassword(string oldPassword, string newPassword, string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException("User is not found");
            }
            if (!_passwordHasher.Verify(oldPassword, user.Passwordhash))
            {
                throw new ConfRadarAuthenticationException("Invalid old password");
            }
            user.Passwordhash = _passwordHasher.Hash(newPassword);
            await _unitOfWork.UserRepository.UpdateUserAsync(user);

        }
    }
}
