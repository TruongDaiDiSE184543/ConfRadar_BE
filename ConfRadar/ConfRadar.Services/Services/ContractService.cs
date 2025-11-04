using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
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
    }
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        public ContractService(IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<ObjectStorageSettings> objectStorageSettings, IObjectStorageFileService objectStorageFileService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
        }

        public async Task<int> CreateReviewerContract(CreateReviewerContractRequest request)
        {

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(request.ConferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Hội nghị với mã {request.ConferenceId} không tồn tại trong hệ thống");
            }
            if (conference.EndDate == null || conference.EndDate < ExtensionHelper.GetVietnamDate())
            {
                throw new BadRequestException("Hội nghị này đã kết thúc, bạn không thể tạo hợp đồng cho reviewer này");
            }
            var reviewer = await _unitOfWork.UserRepository.GetUserByUserId(request.ReviewerId);
            if (reviewer == null)
            {
                throw new NotFoundException($"Reviewer với mã {request.ReviewerId} không tồn tại trong hệ thống");
            }
            var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            var organizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());
            if (adminRole == null || organizerRole == null)
            {
                throw new NotFoundException("Không tìm thấy các role tương ứng trong hệ thống");
            }
            if (reviewer.UserRoles.Any(ur => ur.RoleId == adminRole.RoleId || ur.RoleId == organizerRole.RoleId))
            {
                throw new BadRequestException("Người này hiện đang là admin || organizer trong hệ thống");

            }
            var reviewContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(request.ReviewerId, request.ConferenceId);
            if (reviewContractFound != null)
            {
                throw new BadRequestException($"Đã tồn tại hợp đồng đổi với reviewer {reviewer.FullName} đối với hội nghị {conference.ConferenceName}");
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
                    throw new BadRequestException("Audio and video files are not allowed for contract upload.");
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
                SignDay = ExtensionHelper.GetVietnamDate(),
                ExpireDay = conference.EndDate,
                Wage = request.Wage,
                ContractUrl = contractFileUrl,
                ConferenceId = request.ConferenceId,
            };
            return await _unitOfWork.ReviewerContractRepository.CreateReviewerContractAsync(newContractObj);
        }

        public async Task<List<ConferenceBelongToReviewContractResponse>> GetListConferenceBelongToReviewContractByUserId(string userId)
        {
            return await _unitOfWork.ReviewerContractRepository.GetListConferenceBelongToReviewContractByUserId(userId);
        }

        public async Task<List<PaperDetailBelongToConferenceInReviewContractResposne>> GetPapersBelongToAConferenceByConferenceIdAndUserId(string conferenceId, string userId)
        {
            return await _unitOfWork.ReviewerContractRepository.GetPapersBelongToAConferenceByConferenceIdAndUserId(conferenceId, userId);
        }
    }
}
