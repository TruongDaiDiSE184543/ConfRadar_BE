using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.User;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using ConfRadar.Shared.DTO.Collaborator;
using ConfRadar.Shared.DTO.User;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Options;
using System.Data;
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
        Task<int> ActivateAccount(string userId);
        Task<int> SuspendAccount(string userId);

        Task<int> UpdateProfile(ProfileUpdateRequest request, string userId);
        Task<UserDetailResponse> ViewUserDetail(string userId);
        Task<List<ListUserDetailForAdminAndOrganizerResponse>> ListUserForAdminAndOrganizer();
        Task<int> CreateCollaboratorAccount(CreateCollaboratorAccountRequest request);
        Task<List<GetUsersForCollaboratorCreateResponse>> GetUsersForCollaboratorCreate();
        Task<List<AvailableCustomerResponse>> GetAvailableCustomer();
        Task<List<ReviewerDetailResponse>> ListAllReviewer();
        Task<int> SuspendExternalReviewerAccount(string userId);
        Task<int> ActivateExternalReviewerAccount(string userId);

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
        private readonly ITimeProviderService _timeProviderService;
        public AuthService(IPasswordHasher passwordHasher, IEmailService emailService, ITokenService tokenService, IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService, IOptions<ObjectStorageSettings> objectStorageSettings, IFirebaseAuthService firebaseAuthService, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _objectStorageFileService = objectStorageFileService;
            _objectStorageSettings = objectStorageSettings.Value;
            _firebaseAuthService = firebaseAuthService;
            _timeProviderService = timeProviderService;
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
            var userCreated = UserMapper.FromCreateUserRequestToUser(request, await _timeProviderService.GetVietnamTime());
            userCreated.PasswordHash = hashedPassword;
            userCreated.VerificationToken = verificationToken;
            userCreated.LoginProvider = LoginProviderEnum.Local.ToString();
            userCreated.VerificationTokenExpiry = (await _timeProviderService.GetVietnamTime()).AddDays(1);
            userCreated.AvatarUrl = fileUrl;
            var role = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Customer.GetDescription());
            var userRole = new UserRole()
            {
                UserId = userCreated.UserId,
                RoleId = role!.RoleId,
                AssignedAt = await _timeProviderService.GetVietnamTime(),
                IsActive = true
            };
            var userWallet = new Wallet()
            {
                WalletId = Guid.NewGuid().ToString(),
                UserId = userCreated.UserId,
                Balance = 0,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                UpdatedAt = null
            };
            userCreated.UserRoles.Add(userRole);
            userCreated.Wallet = userWallet;
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
            var timeNow = await _timeProviderService.GetVietnamTime();
            if (user.VerificationTokenExpiry <= timeNow)
            {
                throw new ConfRadarAuthenticationException("Token is expired");
            }
            //user.IsActive = true;
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
            //if (user.IsActive == false)
            //{
            //    throw new ConfRadarAuthenticationException("User is disabled");
            //}
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
            var timeNow = await _timeProviderService.GetVietnamTime();
            user.LastLogin = timeNow;
            if (request.FirebaseMobileFcmToken != null)
            {
                user.FirebaseMobileFcmToken = request.FirebaseMobileFcmToken;
            }
            if (request.FirebaseWebFcmToken != null)
            {
                user.FirebaseWebFcmToken = request.FirebaseWebFcmToken;
            }
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
            user.PasswordResetTokenExpiry = await _timeProviderService.GetVietnamTime();
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
            var timeNow = await _timeProviderService.GetVietnamTime();
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

            var timeNow = await _timeProviderService.GetVietnamTime();
            if (user == null)
            {
                var role = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Customer.GetDescription());
                var userId = Guid.NewGuid().ToString();
                var userRole = new UserRole()
                {
                    UserId = userId,
                    RoleId = role!.RoleId,
                    AssignedAt = timeNow,
                    IsActive = true,
                };
                var userRoleList = new List<UserRole>();
                userRoleList.Add(userRole);
                user = new User()
                {
                    Email = email,
                    FullName = name,
                    IsEmailConfirmed = true,
                    IsActive = true,
                    LastLogin = timeNow,
                    LoginProvider = LoginProviderEnum.Firebase.ToString(),
                    UserId = userId,
                    AvatarUrl = null,
                    CreatedAt = timeNow,
                    UserRoles = userRoleList,
                    Wallet = new Wallet()
                    {
                        WalletId = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Balance = 0,
                        CreatedAt = await _timeProviderService.GetVietnamTime(),
                        UpdatedAt = null
                    }
                };
                await _unitOfWork.UserRepository.CreateUserAsync(user);
            }
            else
            {
                if (!string.Equals(user.LoginProvider, LoginProviderEnum.Firebase.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ConfRadarAuthenticationException($"This account is registered with provider '{user.LoginProvider}'.");
                }
                //if (user.IsActive == false)
                //{
                //    throw new ConfRadarAuthenticationException("User is disabled");
                //}
                if (request.FirebaseMobileFcmToken != null)
                {
                    user.FirebaseMobileFcmToken = request.FirebaseMobileFcmToken;
                }
                if (request.FirebaseWebFcmToken != null)
                {
                    user.FirebaseWebFcmToken = request.FirebaseWebFcmToken;
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
            var timeNow = await _timeProviderService.GetVietnamTime();
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
        public async Task<int> SuspendAccount(string userId)
        {

            var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            var organizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());
            if (adminRole == null || organizerRole == null)
            {
                throw new NotFoundException("Không tìm thấy các role cho admin và organizer trong hệ thống");
            }
            var listSystemRoleIds = new List<String>()
            {
                adminRole.RoleId,
                organizerRole.RoleId,
            };

            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new BadRequestException("Người dùng không tìm thấy");
            }
            if (user.UserRoles?.Any(ur => listSystemRoleIds.Contains(ur.Role.RoleId)) == true)
            {
                throw new BadRequestException($"Không thể suspend organizer hoặc admin");
            }
            int result = 0;
            user.IsActive = false;
            result += await _unitOfWork.UserRepository.UpdateUserAsync(user);
            if (result > 0)
            {
                await _emailService.SendSuspendTemplateEmailAsync(user.Email, user.FullName, "Account bạn đã bị ngừng", "EmailSuspendAccount.html");
            }

            return result;
        }
        public async Task<int> ActivateAccount(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new BadRequestException("Người dùng không tìm thấy");
            }
            user.IsActive = true;
            return await _unitOfWork.UserRepository.UpdateUserAsync(user);
        }
        public async Task<int> UpdateProfile(ProfileUpdateRequest request, string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException($"Không tìm thấy người dùng với id {userId} trong hệ thống");
            }

            user.FullName = request.FullName;
            user.BirthDay = request.BirthDay;
            user.PhoneNumber = request.PhoneNumber;
            if (request.Gender != null)
            {
                user.Gender = request.Gender.GetDescription();
            }
            string fileUrl = string.Empty;
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
                user.AvatarUrl = fileUrl;
            }
            user.BioDescription = request.BioDescription;
            return await _unitOfWork.UserRepository.UpdateUserAsync(user);
        }
        public async Task<UserDetailResponse> ViewUserDetail(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException($"Không tìm thấy người dùng với id {userId} trong hệ thống");
            }
            return new UserDetailResponse()
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                BirthDay = user.BirthDay,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                AvatarUrl = user.AvatarUrl,
                BioDescription = user.BioDescription,
                CreatedAt = user.CreatedAt,
            };
        }

        public async Task<List<ListUserDetailForAdminAndOrganizerResponse>> ListUserForAdminAndOrganizer()
        {
            var userList = await _unitOfWork.UserRepository.GetListUser();
            //var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            //var organizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());

            //var filteredUsers = userList.Where(u => !u.UserRoles.Any(ur => ur.RoleId == adminRole.RoleId || ur.RoleId == organizerRole.RoleId)).ToList();

            var userRolePais = userList.SelectMany(u => u.UserRoles, (user, userRole) => new { User = user, Role = userRole.Role }).ToList();

            var groupedByRole = userRolePais
                .Where(x => x.Role != null)
                .GroupBy(u => u.Role.RoleId)
                .Select(g => new ListUserDetailForAdminAndOrganizerResponse
                {
                    RoleId = g.Key,
                    RoleName = g.First().Role!.RoleName,
                    Users = g.Select(x => new UserDetailForAdminAndOrganizerResponse
                    {
                        UserId = x.User.UserId,
                        Email = x.User.Email,
                        FullName = x.User.FullName,
                        PhoneNumber = x.User.PhoneNumber,
                        Gender = x.User.Gender,
                        AvatarUrl = x.User.AvatarUrl,
                        CreatedAt = x.User.CreatedAt,
                        IsActive = x.User.IsActive,
                        IsEmailConfirmed = x.User.IsEmailConfirmed
                    }).ToList()
                }).ToList();

            return groupedByRole;
        }

        public async Task<int> CreateCollaboratorAccount(CreateCollaboratorAccountRequest request)
        {
            request.Email = request.Email.Trim().ToLower();
            request.FullName = request.FullName.Trim();
            var userByEmail = await _unitOfWork.UserRepository.GetUserByEmail(request.Email);
            if (userByEmail != null)
            {
                throw new ConfRadarAuthenticationException("Người dùng với email này đã tồn tại");
            }
            var userByName = await _unitOfWork.UserRepository.GetUserByName(request.FullName);
            if (userByName != null)
            {
                throw new ConfRadarAuthenticationException("Người dùng với tên đã tồn tại");
            }

            var hashedPassword = _passwordHasher.Hash(request.Password);
            var verificationToken = _tokenService.GenerateSecureRandomToken();


            string confirmationLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.VerifyForgetPassword + $"?token={verificationToken}";
            string userId = Guid.NewGuid().ToString();
            var userCreated = new User()
            {
                UserId = userId,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = hashedPassword,
                IsActive = true,
                IsEmailConfirmed = true,
                LoginProvider = LoginProviderEnum.Local.ToString(),
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                UserRoles = new List<UserRole>(),
                PasswordResetToken = verificationToken,
                PasswordResetTokenExpiry = (await _timeProviderService.GetVietnamTime()).AddDays(1),
                Wallet = new Wallet()
                {
                    WalletId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Balance = 0,
                    CreatedAt = await _timeProviderService.GetVietnamTime(),
                    UpdatedAt = null
                }
            };
            var collabRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription());
            if (collabRole == null)
            {
                throw new NotFoundException("Role collab không tìm thấy trong hệ thống");
            }
            var userRoleObj = new UserRole()
            {
                AssignedAt = await _timeProviderService.GetVietnamTime(),
                RoleId = collabRole.RoleId,
                UserId = userCreated.UserId,
                IsActive = true
            };
            userCreated.UserRoles.Add(userRoleObj);
            int result = 0;
            result += await _unitOfWork.UserRepository.CreateUserAsync(userCreated);
            if (result > 0)
            {
                await _emailService.SendCreateAccountEmail(request.Email, request.FullName, request.Password, confirmationLink, "Tạo tài khoản cho collaborator", "EmailChangePassword.html");
            }
            return result;
        }

        public async Task<List<AvailableCustomerResponse>> GetAvailableCustomer()
        {
            return await _unitOfWork.UserRepository.GetAvailableCustomer();
        }

        public async Task<List<ReviewerDetailResponse>> ListAllReviewer()
        {
            var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            if (localReviewerRole == null)
            {
                throw new NotFoundException("Local reviewer role không tìm thấy trong hệ thống");
            }
            var localReviewerList = await _unitOfWork.UserRepository.GetReviewerList(localReviewerRole.RoleId);
            var result = localReviewerList.Select(x => new ReviewerDetailResponse()
            {
                UserId = x.UserId,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                AvatarUrl = x.AvatarUrl,
                FullName = x.FullName,
            }).ToList();
            return result;
        }

        public async Task<int> SuspendExternalReviewerAccount(string userId)
        {
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ExternalReviewer.GetDescription());
            if (externalReviewerRole == null)
            {
                throw new Exception("Không tìm thấy role hệ thống");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng với id {userId}");
            }
            var reviewContracts = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);
            if (!reviewContracts.Any())
            {
                throw new BadRequestException($"Không tìm thấy bất cứ reviewer outsourced với tên{user.FullName}");
            }
            var userRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(userId, externalReviewerRole.RoleId);
            if (userRole == null)
            {
                throw new NotFoundException($"Không tìm thấy role cho reviewer");
            }
            if (userRole.IsActive == false)
            {
                throw new BadRequestException($"Người dùng {user.FullName} đã bị disable role reviewer");
            }
            userRole.IsActive = false;
            return await _unitOfWork.UserRoleRepository.UpdateUserRole(userRole);
        }
        public async Task<int> ActivateExternalReviewerAccount(string userId)
        {
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ExternalReviewer.GetDescription());
            if (externalReviewerRole == null)
            {
                throw new Exception("Không tìm thấy role hệ thống");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng với id {userId}");
            }
            var reviewContracts = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);
            if (!reviewContracts.Any())
            {
                throw new BadRequestException($"Không tìm thấy bất cứ reviewer outsourced với tên {user.FullName}");
            }
            var userRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(userId, externalReviewerRole.RoleId);
            if (userRole == null)
            {
                throw new NotFoundException($"Không tìm thấy role cho reviewer");
            }
            if (userRole.IsActive == true)
            {
                throw new BadRequestException($"Người dùng {user.FullName} đã được cấp lại role reviewer");
            }
            userRole.IsActive = true;
            return await _unitOfWork.UserRoleRepository.UpdateUserRole(userRole);
        }


        public async Task<List<GetUsersForCollaboratorCreateResponse>> GetUsersForCollaboratorCreate()
        {
            var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            var organizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());
            var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            var collabRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription());
            if (adminRole == null || organizerRole == null || localReviewerRole == null || collabRole == null)
            {
                throw new Exception("Không tìm thấy một hoặc nhiều role hệ thống");
            }

            var userList = await _unitOfWork.UserRepository.GetListUser();

            var filteredUsers = userList
                .Where(u => !u.UserRoles.Any(ur => ur.RoleId == adminRole.RoleId || ur.RoleId == organizerRole.RoleId || ur.RoleId == collabRole.RoleId))
                .ToList();
            var result = filteredUsers.Select(u => new GetUsersForCollaboratorCreateResponse()
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                BioDescription = u.BioDescription,
            }).ToList();
            return result;
        }
    }
}



