using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        Task<string> SubmitAbstract(CreateAbstractRequest request, string userId);
        Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId);
        Task<int> DecideFullPaperStatus(UpdateFullPaperStatusRequest request, string userId);
        Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId);
        Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId);
        Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request,string userId);
        Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId);

    }
    public class PaperService : IPaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMomoService _momoService;
        private readonly ITokenService _tokenService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        public PaperService(IUnitOfWork unitOfWork, IMomoService momoService, ITokenService tokenService, IOptions<ObjectStorageSettings> objectStorageSettings, IObjectStorageFileService objectStorageFileService)
        {
            _unitOfWork = unitOfWork;
            _momoService = momoService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
        }



        public async Task<string> SubmitAbstract(CreateAbstractRequest request, string userId)
        {
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá hội nghị với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference.IsResearchConference == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference");
            }
            if (conferencePrice.IsAuthor == false)
            {
                throw new BadRequestException($"Giá vé hiện tại không dành cho tác giả, xin hãy chọn mức giá khác");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference tổ chức bởi confradar");
            }
            if (conferencePrice.AvailableSlot <= 0)
            {
                throw new BadRequestException($"Hiện tại slot cho research conference đã hết");
            }
            string abstractFileUrl = string.Empty;
            if (request.AbstractFile != null)
            {
                if (request.AbstractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                using var stream = request.AbstractFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.AbstractFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.abstractfile.ToString(), uniqueFileName, stream, request.AbstractFile.ContentType);
                abstractFileUrl = baseUri + objectStorageFileUrl;
            }
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.MoMo.GetDescription());
            decimal applyPercent = 0;
            var dateNow = ExtensionHelper.GetVietnamDate();
            var validPhases = conferencePrice.PricePhases
            .Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow)
            .OrderBy(p => p.StartDate)
            .ToList();

            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để nộp abstract");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);

            if (currentPhase == null)
            {
                throw new BadRequestException("Tất cả các phase hợp lệ hiện tại đã hết slot");
            }
            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;

            var finalPrice = (long)(conferencePrice.TicketPrice - (conferencePrice.TicketPrice * applyPercent / 100));

            var result = await _momoService.ProcessPaymentForAbstract(request, conferencePrice.ConferenceId, userId, finalPrice, paymentMethod.PaymentMethodId, sessionIdsList, abstractFileUrl, $"Thanh toán abstract");
            return result;
        }

        public async Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId)
        {
            var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            if (acceptedStatus == null || pendingReviewStatus == null || currentFullPaperPhase == null)
            {
                throw new NotFoundException($"Không thể tìm thấy các trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByPaperIdAndUserIdAsync(request.PaperId, userId);
            if (paper == null)
            {
                throw new BadRequestException($"Không thể tìm thấy paper id: {request.PaperId} cho user {userId} hiện tại");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không thể cập nhật full paper vì trạng thái hiện tại không phải full paper");
            }
            if (string.IsNullOrWhiteSpace(paper.FullPaperId))
            {
                throw new BadRequestException($"Paper id {request.PaperId} chưa có full paper để cập nhật.");
            }
            if (!paper.FullPaperId.Equals(request.FullPaperId, StringComparison.Ordinal))
            {
                throw new BadRequestException("Full paper id không khớp với paper đã nộp.");
            }
            var abstractSubmission = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId);
            if (abstractSubmission == null)
            {
                throw new BadRequestException($"Không thể tìm thấy dữ liệu abstract");
            }
            if (abstractSubmission.GlobalStatusId != acceptedStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Abstract phải được chấp nhận (Accepted) trước khi nộp full paper.");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Không thể tìm thấy fullpaper id {request.FullPaperId}");
            }
            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException("Full paper không ở trạng thái Pending, không thể cập nhật được.");
            }
            string fullPaperFileUrl = string.Empty;
            if (request.FullPaperFile != null)
            {
                if (request.FullPaperFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }
                using var stream = request.FullPaperFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.FullPaperFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.fullpaperfile.ToString(), uniqueFileName, stream, request.FullPaperFile.ContentType);
                fullPaperFileUrl = baseUri + objectStorageFileUrl;
                fullPaper.FullPaperUrl = fullPaperFileUrl;
            }
            return await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
        }
        public async Task<int> DecideFullPaperStatus(UpdateFullPaperStatusRequest request, string userId)
        {
            if (request.ReviewStatus == ReviewStatusEnum.Pending)
            {
                throw new BadRequestException("Không thể chuyển trạng thái full paper status Pending.");
            }
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            if (pendingReviewStatus == null || currentFullPaperPhase == null)
            {
                throw new NotFoundException($"Không thể tìm thấy các trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không tìm thấy paper với id {request.PaperId}.");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper với id {request.FullPaperId} không tìm thấy");
            }
            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper với id phải là trạng thái (Pending) để được cập nhật");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phải đang trong full paper phase để có thể cập nhật trạng thái");
            }
            var paperReviewerList = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            if (paperReviewerList == null || paperReviewerList.Count <= 0)
            {
                throw new NotFoundException($"Không tìm thấy các danh sách gán reviewer cho bài báo này");
            }
            var headPaperReviewer = paperReviewerList.FirstOrDefault(x => x.IsHeadReviewer == true && x.UserId == userId);
            if (headPaperReviewer == null)
            {
                throw new NotFoundException($"Không tìm thấy bạn là head reviewer trong danh sách gán reviewer.");
            }
            switch (request.ReviewStatus)
            {
                case ReviewStatusEnum.Accepted:
                    var acceptedStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
                    fullPaper.ReviewStatusId = acceptedStatus.ReviewStatusId;

                    var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
                    paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;

                    var pendingGlobal = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

                    var cameraReady = new CameraReady
                    {
                        CameraReadyId = Guid.NewGuid().ToString(),
                        GlobalStatusId = pendingGlobal.GlobalStatusId,
                        CameraReadyUrl = null,
                    };
                    paper.CameraReadyId = cameraReady.CameraReadyId;
                    await _unitOfWork.CameraReadyRepository.CreateAsync(cameraReady);
                    break;
                case ReviewStatusEnum.Rejected:

                    var rejectedStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
                    fullPaper.ReviewStatusId = rejectedStatus.ReviewStatusId;
                    break;
                case ReviewStatusEnum.Revise:

                    var revisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revision.GetDescription());
                    var reviseStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Revise.GetDescription());
                    var pendingGlobalRevise = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

                    fullPaper.ReviewStatusId = reviseStatus.ReviewStatusId;
                    paper.PaperPhaseId = revisePhase.PaperPhaseId;

                    var revisionPaperObj = new RevisionPaper()
                    {
                        RevisionPaperId = Guid.NewGuid().ToString(),
                        RevisionRound = null,
                        GlobalStatusId = pendingGlobalRevise.GlobalStatusId,
                    };
                    paper.RevisionPaperId = revisionPaperObj.RevisionPaperId;
                    await _unitOfWork.RevisionPaperRepository.CreateRevisionPaperAsync(revisionPaperObj);
                    break;
            }
            var result1 = await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
            var result2 = await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
            return result1 + result2;


        }

        public async Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId)
        {
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revision.GetDescription());
            if (currentRevisePhase == null)
            {
                throw new NotFoundException($"Không thể tìm thấy trạng thái revise phase trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm thấy trong hệ thống");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phải trong trạng thái revise để thực hiện gửi file");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new BadRequestException($"Hệ thống không thể tìm thấy revise phase tương ứng");
            }
            if (paper.PresenterId != userId)
            {
                throw new ConfRadarAuthenticationException("Bạn không có quyền nộp revision cho bài báo này");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new BadRequestException($"Revision paper id {request.RevisionPaperId} không tìm thấy trong hệ thống");
            }

            var totalRevisionRoundAllowed = paper.Conference.ResearchConferenceDetail.RevisionAttemptAllowed;
            var totalRevisionPaperCount = revisionPaper.RevisionPaperSubmissions.Count;
            if (totalRevisionPaperCount >= totalRevisionRoundAllowed)
            {
                throw new BadRequestException($"Không thể nộp thêm paper submission vì đã quá {totalRevisionRoundAllowed}, Xin vui lòng chờ phán quyết từ head reviewer!");
            }
            if (revisionPaper.RevisionRound == 0)
            {
                revisionPaper.RevisionRound = 1;
            }
            else
            {
                revisionPaper.RevisionRound = revisionPaper.RevisionRound + 1;
            }
            string? revisionFileUrl = null;
            if (request.RevisionPaperFile != null)
            {
                if (request.RevisionPaperFile.ContentType == null)
                {
                    throw new BadRequestException("Content type không hợp lệ");
                }

                using var stream = request.RevisionPaperFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.RevisionPaperFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.revisionpaperfile.ToString(), uniqueFileName, stream, request.RevisionPaperFile.ContentType);
                revisionFileUrl = baseUri + objectStorageFileUrl;
            }
            var revisionPaperSubmissionObj = new RevisionPaperSubmission()
            {

                RevisionPaperSubmissionId = Guid.NewGuid().ToString(),
                RevisionPaperId = request.RevisionPaperId,
                RevisionDeadlineRoundId = request.RevisionDeadlineRoundId,
                RevisionPaperUrl = revisionFileUrl,
            };
            var result1 = await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
            var result2 = await _unitOfWork.RevisionPaperSubmissionRepository.CreateRevisionPaperSubmissionAsync(revisionPaperSubmissionObj);
            return result1 + result2;
        }

        public async Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId)
        {
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null) 
            {
                throw new NotFoundException($"Không tìm thấy paper  id {request.PaperId} trong hệ thống");

            }

            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper submission id {request.RevisionPaperSubmissionId} trong hệ thống");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(request.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Không tìm thấy user với id {userId} trong hệ thống assign cho bài báo {request.PaperId}");
            }
            var feedBackList = new List<RevisionSubmissionFeedback>();
            foreach(var feedback in request.Feedbacks)
            {
                var feedbackObj = new RevisionSubmissionFeedback()
                {
                    RevisionSubmissionFeedbackId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Feedback = feedback.Feedback,
                    Response = null,
                    SortOrder = feedback.SortOrder,
                    CreatedAt = ExtensionHelper.GetVietnamTime(),
                    RevisionPaperSubmissionId = revisionPaperSubmission.RevisionPaperSubmissionId,
                };
                feedBackList.Add(feedbackObj);
            }
            return await _unitOfWork.RevisionSubmissionFeedbackRepository.CreateMultipleFeedbacksAsync(feedBackList);
        }

        public async Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request, string userId)
        {
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper  id {request.PaperId} trong hệ thống");

            }
            if (paper.PresenterId != userId)
            {
                throw new BadRequestException($"Bạn không thể gửi phản hồi vì bạn không phải là chủ bài báo này");
            }
            var feedBackList = new List<RevisionSubmissionFeedback>();
            foreach (var response in request.Responses)
            {
                var revisionSubmissionFeedback = await _unitOfWork.RevisionSubmissionFeedbackRepository.GetFeedbackByIdAsync(response.RevisionSubmissionFeedbackId);
                if (revisionSubmissionFeedback == null)
                {
                    throw new NotFoundException($"Không tìm thấy paper  id {response.RevisionSubmissionFeedbackId} trong hệ thống");
                }
                revisionSubmissionFeedback.Response = response.Response;
                feedBackList.Add(revisionSubmissionFeedback);
            }

            return await _unitOfWork.RevisionSubmissionFeedbackRepository.UpdateMultipleFeedbacksAsync(feedBackList);
        }

        public Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
