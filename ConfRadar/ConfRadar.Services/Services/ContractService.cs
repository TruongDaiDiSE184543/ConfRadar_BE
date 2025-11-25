using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.Contract;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.ReviewContract;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IContractService
    {
        Task<List<ConferenceBelongToReviewContractResponse>> GetListConferenceBelongToReviewContractByUserId(string userId);
        Task<List<PaperDetailBelongToConferenceInReviewContractResposne>> GetPapersBelongToAConferenceByConferenceIdAndUserId(string conferenceId, string userId);
        Task<int> CreateReviewerContract(CreateReviewerContractRequest request);
        Task<int> CreateReviewerContractForNewUser(CreateReviewerContractForNewUserRequest request);
        Task<List<GetUsersForReviewerContractResponse>> GetUsersForReviewerContract(GetUsersForReviewerContractRequest request);
        Task<List<OwnContractDetailResponse>> GetListOwnContract(string userId);
        Task<List<ContractDetailResponseForOrganizer>> GetListContractByReviewerId(string reviewerId);
        Task<UserExternalContractCount> GetOwnContractCount(string userId);
        Task<OwnActiveContractDetailResponse> GetUserActiveExternalContract(string userId);
        Task<int> CreateCollaboratorContract(CollaboratorContractRequest request);
        Task<PagedResultResponseDto<CollaboratorContractResponse>> GetListCollaboratorContract(CollaboratorContractSearchParam request);
        Task<UserExternalWageTotal> GetUserExternalWageTotal(string userId);
    }
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITimeProviderService _timeProviderService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        public ContractService(IUnitOfWork unitOfWork, ITokenService tokenService,
            IOptions<ObjectStorageSettings> objectStorageSettings, IObjectStorageFileService objectStorageFileService,
            ITimeProviderService timeProviderService, IPasswordHasher passwordHasher, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
            _timeProviderService = timeProviderService;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<int> CreateReviewerContract(CreateReviewerContractRequest request)
        {
            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ExternalReviewer.GetDescription());
            if (externalReviewerRole == null)
            {
                throw new NotFoundException("Không tìm thấy external reviewer role trong hệ thống");
            }
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(request.ConferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Hội nghị với mã {request.ConferenceId} không tồn tại trong hệ thống");
            }
            if (conference.EndDate == null || conference.EndDate < dateNow)
            {
                throw new BadRequestException("Hội nghị này đã kết thúc, bạn không thể tạo hợp đồng cho reviewer này");
            }
            var reviewer = await _unitOfWork.UserRepository.GetUserByUserId(request.ReviewerId);
            if (reviewer == null)
            {
                throw new NotFoundException($"Reviewer với mã {request.ReviewerId} không tồn tại trong hệ thống");
            }

            var reviewContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(request.ReviewerId, request.ConferenceId);
            if (reviewContractFound != null)
            {
                throw new BadRequestException($"Đã tồn tại hợp đồng đổi với reviewer {reviewer.FullName} đối với hội nghị {conference.ConferenceName}");
            }
            GetUsersForReviewerContractRequest check = new GetUsersForReviewerContractRequest()
            {
                ConferenceId = request.ConferenceId,
            };
            var listUserForReviewer = await GetUsersForReviewerContract(check);
            var validReviewer = listUserForReviewer.FirstOrDefault(r => r.UserId == request.ReviewerId);
            if (validReviewer == null)
            {
                throw new BadRequestException("Người này hiện tại không đáp ứng đủ nhu cầu. Có thể là do là người trong hệ thống, là người review trong hội nghị này hoặc đã sỡ hữu bài báo nào đó trong hội nghị này");
            }
            string contractFileUrl = null;
            if (request.ContractFile != null)
            {
                if (request.ContractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                if (request.ContractFile.ContentType.StartsWith("audio/") || request.ContractFile.ContentType.StartsWith("video/"))
                {
                    throw new BadRequestException("Audio và video không phù hợp để tạo hợp đồng.");
                }

                using var stream = request.ContractFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ContractFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.reviewercontractfile.ToString(), uniqueFileName, stream, request.ContractFile.ContentType);
                contractFileUrl = baseUri + objectStorageFileUrl;
            }
            var newContractObj = new ReviewerContract()
            {
                ReviewerContractId = Guid.NewGuid().ToString(),
                UserId = request.ReviewerId,
                IsActive = true,
                SignDay = request.SignDay,
                ExpireDay = conference.EndDate,
                Wage = request.Wage,
                ContractUrl = contractFileUrl,
                ConferenceId = request.ConferenceId,
            };
            var userExternalRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(reviewer.UserId, externalReviewerRole.RoleId);
            if (userExternalRole == null)
            {
                var userRoleObj = new UserRole()
                {
                    AssignedAt = timeNow,
                    RoleId = externalReviewerRole.RoleId,
                    UserId = reviewer.UserId,
                    IsActive = true
                };
                await _unitOfWork.UserRoleRepository.CreateUserRoleAsync(userRoleObj);
            }
            return await _unitOfWork.ReviewerContractRepository.CreateReviewerContractAsync(newContractObj);
        }



        public async Task<int> CreateReviewerContractForNewUser(CreateReviewerContractForNewUserRequest request)
        {
            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ExternalReviewer.GetDescription());
            var customerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Customer.GetDescription());
            if (externalReviewerRole == null || customerRole == null)
            {
                throw new NotFoundException("Không tìm thấy external reviewer || customer role trong hệ thống");
            }
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(request.ConferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Hội nghị với mã {request.ConferenceId} không tồn tại trong hệ thống");
            }
            if (conference.EndDate == null || conference.EndDate < dateNow)
            {
                throw new BadRequestException("Hội nghị này đã kết thúc, bạn không thể tạo hợp đồng cho reviewer này");
            }
            var reviewer = await _unitOfWork.UserRepository.GetUserByEmail(request.Email);
            if (reviewer != null)
            {
                throw new BadRequestException($" Email {request.Email} đã tồn tại trong hệ thống");
            }
            var userByName = await _unitOfWork.UserRepository.GetUserByName(request.FullName);
            if (userByName != null)
            {
                throw new ConfRadarAuthenticationException("Người dùng với tên đã tồn tại");
            }
            string contractFileUrl = null;
            if (request.ContractFile != null)
            {
                if (request.ContractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                if (request.ContractFile.ContentType.StartsWith("audio/") || request.ContractFile.ContentType.StartsWith("video/"))
                {
                    throw new BadRequestException("Audio và video không phù hợp để tạo hợp đồng.");
                }

                using var stream = request.ContractFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ContractFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.reviewercontractfile.ToString(), uniqueFileName, stream, request.ContractFile.ContentType);
                contractFileUrl = baseUri + objectStorageFileUrl;
            }

            //var hashedPassword = _passwordHasher.Hash(request.Password);
            var verificationToken = _tokenService.GenerateSecureRandomToken();
            string confirmationLink = ConfRadarDomain.Url + ConfRadarApiEndPoint.VerifyForgetPassword + $"?token={verificationToken}";
            var userCreated = new User()
            {
                UserId = Guid.NewGuid().ToString(),
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = timeNow,
            };
            userCreated.PasswordHash = null;
            userCreated.VerificationToken = verificationToken;
            userCreated.LoginProvider = LoginProviderEnum.Local.ToString();
            userCreated.VerificationTokenExpiry = timeNow.AddDays(1);
            var usercustomerRole = new UserRole()
            {
                UserId = userCreated.UserId,
                RoleId = customerRole!.RoleId,
                AssignedAt = timeNow,
                IsActive = true
            };
            var userExternalRole = new UserRole()
            {
                UserId = userCreated.UserId,
                RoleId = externalReviewerRole!.RoleId,
                AssignedAt = timeNow,
                IsActive = true
            };
            userCreated.UserRoles.Add(usercustomerRole);
            userCreated.UserRoles.Add(userExternalRole);

            var userWallet = new Wallet()
            {
                WalletId = Guid.NewGuid().ToString(),
                UserId = userCreated.UserId,
                Balance = 0,
                CreatedAt = timeNow,
                UpdatedAt = null
            };
            userCreated.Wallet = userWallet;



            var newContractObj = new ReviewerContract()
            {
                ReviewerContractId = Guid.NewGuid().ToString(),
                UserId = userCreated.UserId,
                IsActive = true,
                SignDay = request.SignDay,
                ExpireDay = conference.EndDate,
                Wage = request.Wage,
                ContractUrl = contractFileUrl,
                ConferenceId = request.ConferenceId,
            };
            userCreated.ReviewerContracts.Add(newContractObj);

            int result = 0;
            result += await _unitOfWork.UserRepository.CreateUserAsync(userCreated);
            if (result > 0)
            {
                await _emailService.SendCreateAccountEmail(request.Email, request.FullName, confirmationLink, "Tạo tài khoản cho reviewer outsourced", "EmailChangePassword.html");
            }
            return result;
        }










        public async Task<List<ConferenceBelongToReviewContractResponse>> GetListConferenceBelongToReviewContractByUserId(string userId)
        {
            return await _unitOfWork.ReviewerContractRepository.GetListConferenceBelongToReviewContractByUserId(userId);
        }



        public async Task<List<PaperDetailBelongToConferenceInReviewContractResposne>> GetPapersBelongToAConferenceByConferenceIdAndUserId(string conferenceId, string userId)
        {
            return await _unitOfWork.ReviewerContractRepository.GetPapersBelongToAConferenceByConferenceIdAndUserId(conferenceId, userId);
        }

        public async Task<List<GetUsersForReviewerContractResponse>> GetUsersForReviewerContract(GetUsersForReviewerContractRequest request)
        {
            var conferenceOrganizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());
            var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            if (conferenceOrganizerRole == null || adminRole == null)
            {
                throw new NotFoundException("Không tìm thấy các role tương ứng trong hệ thống");
            }
            List<string> roleIds = new List<string>();
            roleIds.Add(conferenceOrganizerRole.RoleId);
            roleIds.Add(adminRole.RoleId);
            return await _unitOfWork.ReviewerContractRepository.GetUsersForReviewerContract(request.ConferenceId, roleIds);
        }
        public async Task<List<OwnContractDetailResponse>> GetListOwnContract(string userId)
        {
            var ownReviewerContract = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);
            var result = ownReviewerContract.Select(rc => new OwnContractDetailResponse()
            {
                ReviewerContractId = rc.ReviewerContractId,
                IsActive = rc.IsActive,
                SignDay = rc.SignDay,
                ExpireDay = rc.ExpireDay,
                Wage = rc.Wage,
                ContractUrl = rc.ContractUrl,
                ConferenceId = rc.ConferenceId,
                ConferenceName = rc.Conference?.ConferenceName,
                ConferenceDescription = rc.Conference?.Description,
                ConferenceBannerImageUrl = rc.Conference?.BannerImageUrl,

            }).ToList();
            return result;
        }

        public async Task<List<ContractDetailResponseForOrganizer>> GetListContractByReviewerId(string reviewerId)
        {
            var reviewerContract = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(reviewerId);
            var result = reviewerContract.Select(rc => new ContractDetailResponseForOrganizer()
            {
                ReviewerContractId = rc.ReviewerContractId,
                UserId = rc.UserId,
                Email = rc.User?.Email,
                FullName = rc.User?.FullName,
                AvatarUrl = rc.User?.AvatarUrl,
                IsActive = rc.IsActive,
                SignDay = rc.SignDay,
                ExpireDay = rc.ExpireDay,
                Wage = rc.Wage,
                ContractUrl = rc.ContractUrl,
                ConferenceId = rc.ConferenceId,
                ConferenceName = rc.Conference?.ConferenceName,
                ConferenceDescription = rc.Conference?.Description,
                ConferenceBannerImageUrl = rc.Conference?.BannerImageUrl,
            }).ToList();
            return result;
        }

        public async Task<UserExternalContractCount> GetOwnContractCount(string userId)
        {
            var contracts =  await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);
            return new UserExternalContractCount()
            {
                ContractCount = contracts.Count(),
                ContractDetail = contracts.Select(rc => new OwnContractDetailResponse()
                {
                    ReviewerContractId = rc.ReviewerContractId,
                    IsActive = rc.IsActive,
                    SignDay = rc.SignDay,
                    ExpireDay = rc.ExpireDay,
                    Wage = rc.Wage,
                    ContractUrl = rc.ContractUrl,
                    ConferenceId = rc.ConferenceId,
                    ConferenceName = rc.Conference?.ConferenceName,
                    ConferenceDescription = rc.Conference?.Description,
                    ConferenceBannerImageUrl = rc.Conference?.BannerImageUrl,
                    
                }).ToList()
            };
        }
        public async Task<OwnActiveContractDetailResponse> GetUserActiveExternalContract(string userId)
        {
            var ownReviewerContract = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);
            var activeReviewerContract = ownReviewerContract.Where(rc => rc.IsActive == true);
            var result = new OwnActiveContractDetailResponse()
            {
                ActiveContractCount = activeReviewerContract.Count(),
                ContractDetail = activeReviewerContract.Select(rc => new OwnContractDetailResponse()
                {
                    ReviewerContractId = rc.ReviewerContractId,
                    IsActive = rc.IsActive,
                    SignDay = rc.SignDay,
                    ExpireDay = rc.ExpireDay,
                    Wage = rc.Wage,
                    ContractUrl = rc.ContractUrl,
                    ConferenceId = rc.ConferenceId,
                    ConferenceName = rc.Conference?.ConferenceName,
                    ConferenceDescription = rc.Conference?.Description,
                    ConferenceBannerImageUrl = rc.Conference?.BannerImageUrl,
                }).ToList()
            };
            return result;
        }

        public async Task<int> CreateCollaboratorContract(CollaboratorContractRequest request)
        {
            var collabRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription());
            if (collabRole == null)
                throw new NotFoundException("Không tìm thấy role trong hệ thống");

            var user = await _unitOfWork.UserRepository.GetUserByUserId(request.UserId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng");

            if (!user.UserRoles.Any(ur => ur.RoleId == collabRole.RoleId))
                throw new BadRequestException("Người dùng này không phải collaborator");

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(request.ConferenceId);
            if (conference == null)
                throw new NotFoundException($"Không tìm thấy hội nghị với mã {request.ConferenceId}");



            var collabContract = await _unitOfWork.CollaboratorContractRepository.GetListCollaboratorContractByUserIdAsync(request.UserId);
            var currentCollabContract = collabContract.FirstOrDefault(cc => cc.ConferenceId == request.ConferenceId && cc.UserId == request.UserId);
            if (currentCollabContract != null)
            {
                throw new BadRequestException($"Đã tìm thấy hợp đồng cho collaborator {currentCollabContract.CollaboratorContractId} của {currentCollabContract.User!.FullName} được kí vào ngày {currentCollabContract.SignDay}");
            }

            if (request.IsTicketSelling == true)
            {
                if (request.IsPriceStep == false || request.IsSessionStep == false)
                    throw new BadRequestException("price step và session step là bắt buộc cho hội nghị bán vé");
                if (request.Commission == null || request.Commission <= 0)
                    throw new BadRequestException("commission không được bỏ trống hoặc có giá trị <=0");

            }
            if (request.SignDay > request.FinalizePaymentDate)
                throw new BadRequestException("Ngày kí hợp đồng phải trước ngày giải ngân");



            string fileUrl = null;
            if (request.ContractFile != null)
            {
                if (request.ContractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }

                using var stream = request.ContractFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ContractFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.collaboratorcontract.ToString(), uniqueFileName, stream, request.ContractFile.ContentType);
                fileUrl = baseUri + objectStorageFileUrl;
            }

            var collabContractObj = new CollaboratorContract()
            {
                CollaboratorContractId = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                IsSponsorStep = request.IsSponsorStep,
                IsMediaStep = request.IsMediaStep,
                IsPolicyStep = request.IsPolicyStep,
                IsSessionStep = request.IsSessionStep,
                IsPriceStep = request.IsPriceStep,
                IsTicketSelling = request.IsTicketSelling,
                IsClosed = false,
                SignDay = request.SignDay,
                FinalizePaymentDate = request.FinalizePaymentDate,
                Commission = request.Commission,
                ConferenceId = request.ConferenceId,
                ContractUrl = fileUrl,
            };
            return await _unitOfWork.CollaboratorContractRepository.CreateCollaboratorContractAsync(collabContractObj);


        }


        public async Task<PagedResultResponseDto<CollaboratorContractResponse>> GetListCollaboratorContract(CollaboratorContractSearchParam request)
        {
            return await _unitOfWork.CollaboratorContractRepository.GetListCollaboratorContractWithFilter(request);

        }

        public async Task<UserExternalWageTotal> GetUserExternalWageTotal(string userId)
        {
            var reviewerContracts = await _unitOfWork.ReviewerContractRepository.GetReviewerContractsByUserIdAsync(userId);

            return new UserExternalWageTotal
            {
                Wage = reviewerContracts.Sum(rc => rc.Wage),
                ContractDetail = reviewerContracts.Select(rc => new OwnContractDetailResponse()
                {
                    ReviewerContractId = rc.ReviewerContractId,
                    IsActive = rc.IsActive,
                    SignDay = rc.SignDay,
                    ExpireDay = rc.ExpireDay,
                    Wage = rc.Wage,
                    ContractUrl = rc.ContractUrl,
                    ConferenceId = rc.ConferenceId,
                    ConferenceName = rc.Conference?.ConferenceName,
                    ConferenceDescription = rc.Conference?.Description,
                    ConferenceBannerImageUrl = rc.Conference?.BannerImageUrl,
                    
                }).ToList()
            };
        }
    }
}
