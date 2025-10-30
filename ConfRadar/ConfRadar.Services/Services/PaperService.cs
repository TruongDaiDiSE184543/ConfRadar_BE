using ConfRadar.Repositories;
using ConfRadar.Repositories.DTO.Abstract;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.FullPaperReview;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ReviewStatus = ConfRadar.Repositories.Models.ReviewStatus;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        Task<int> SubmitAbstract(CreateAbstractRequest request, string userId);
        Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId);
        Task<List<PendingAbstractResponse>> GetListPendingAbstract();

        //Task<FullPaperResponse> SubmitFullPaper (CreateFullPaperRequest request, string userId);
        Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId);
        //cho head reviewer quyết định cuối cùng
        Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId);
        
        //Task<int> (UpdateFullPaperReviewStatusRequest request, string userId);

        //gửi review  cho head reviewer xem
        Task<string> SubmitReviewForFullPaper(CreateFullPaperReviewRequest request, string userId);
        Task<List<FullPaperReviewResponse>> GetFullPaperReviewsByFullPaperId(string fullPaperId);


        Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId);
        Task<int> DecideReviseStatus(UpdateRevisionStatusRequest request, string userId);
        Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId);
        Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request,string userId);
        Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId);
        Task<List<RevisionPaperReviewResponse>> ListRevisionPaperReview(ListRevisionPaperReviewRequest request, string userId);
        Task<List<PapersAssignedToReviewerResponse>> GetAllAssignedPapersToAReviewer(string userId, string conferenceId);




        Task<string> CreateCameraReady(CreateCameraReadyRequest request, string userId);
        Task<int> UpdateCameraReady(UpdateCameraReadyRequest request, string userId);
       
        Task<int> DecideCameraReadyStatus(UpdateCameraReadyStatusRequest request, string userId);
        Task <List<Paper>> GetSubmittedPaper(string userId);
        Task<PaperDetailReponse> getPaperDetail(string paperId);



        Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases();

        Task<List<PaperDetailResponseDTO>> GetListAllPaper();

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

        public async Task<int> SubmitAbstract(CreateAbstractRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var paperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (paperPhase == null || pendingGlobalStatus== null) 
            {
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper==null)
            {
                throw new NotFoundException($"Không tìm thấy paper với id {request.PaperId} trong hệ thống");
            }
            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không đang trong quá trình gửi abstract");
            }
            if (paper.PresenterId != userId)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            }

            var submitterReviewContracts = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            foreach (var coauthorId in request.CoAuthorId)
            {
                if (coauthorId == userId)
                {
                    throw new BadRequestException("Bạn không thể thêm chính mình làm co-author.");
                }

                bool isCoauthorReviewerInPaperReviewer = submitterReviewContracts
                    .Any(pr => pr.UserId == coauthorId);
                var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(coauthorId, paper.Conference!.ConferenceId);
                
                if (reviewerContractFound != null)
                {
                    if (reviewerContractFound.IsActive ==true)
                    {
                        throw new BadRequestException($"Co author với id {coauthorId} hiện đang có hợp đồng reviewer");
                    }
                }
                if (isCoauthorReviewerInPaperReviewer == true)
                {
                    throw new BadRequestException($"Người dùng {coauthorId} đang là reviewer của bài báo này, không thể thêm làm co-author.");
                }
            }

            if (paper.AbstractId != null)
            {
                throw new BadRequestException("Paper này đã có abstract được nộp rồi");
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
            var abstractObj = new Abstract()
            {
                AbstractId = Guid.NewGuid().ToString(),
                AbstractUrl = abstractFileUrl,
                GlobalStatusId = pendingGlobalStatus.GlobalStatusId,
            };
            paper.AbstractId = abstractObj.AbstractId;


            List<PaperAuthor> paperAuthorList = new List<PaperAuthor>();
            foreach (var coAuthor in request.CoAuthorId)
            {
                var paperAuthorObj = new PaperAuthor()
                {
                    IsPresenter = false,
                    UserId = coAuthor,
                    PaperId = request.PaperId,
                };
                paperAuthorList.Add(paperAuthorObj);
            }
            
            

            int finalResult;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result1 = await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                var result2 =  await _unitOfWork.AbstractRepository.CreateAbstractAsync(abstractObj);
                var result3 = 0;
                if (paperAuthorList.Count > 0)
                {
                    result3 = await _unitOfWork.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(paperAuthorList);
                }
                finalResult = result1 + result2 + result3;
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            return finalResult;
        }
        public async Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId)
        {
            if (request.GlobalStatus.Equals(GlobalStatusEnum.Pending))
            {
                throw new BadRequestException($"Không thể truyển trạng thái pending cho abstract");
            }
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

            var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription());
            var abstractPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (abstractPaperPhase == null || pendingGlobalStatus == null || rejectedGlobalStatus==null || acceptedGlobalStatus==null|| fullPaperPhase ==null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (basePaper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper với id {request.PaperId} trong hệ thống");
            }
            if (basePaper.PaperPhaseId != abstractPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không đang trong quá trình quyết định abstract");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(request.AbstractId);
            if (abstractPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy abstract paper với id {request.AbstractId} trong hệ thống");
            }
            if (abstractPaper.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Abstract hiện tại không đang trong trạng thái pending, vui lòng thử lại sau");
            }
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        abstractPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        basePaper.PaperPhaseId = fullPaperPhase.PaperPhaseId;
                        break;
                    case GlobalStatusEnum.Rejected:
                        abstractPaper.GlobalStatusId = rejectedGlobalStatus.GlobalStatusId;

                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }
                result += await _unitOfWork.AbstractRepository.UpdateAbstractAsync(abstractPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(basePaper);
                await _unitOfWork.CommitAsync();
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            return result;
        }



        //public async Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId)
        //{
        //    if (request.PaperId == null) throw new Exception("Cần có paperid để nộp fullpaper");
        //    var PaperBase = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
        //    if (PaperBase == null) throw new Exception($"Không tìm thấy paper với id{request.PaperId}");
        //    string fullPaperURL = string.Empty;
        //    if(request.FullPaperFile != null)
        //    {
        //        if (request.FullPaperFile.ContentType == null) throw new Exception("Không có dữ liệu file đầu vào để nộp");
        //        using var stream = request.FullPaperFile.OpenReadStream();
        //        var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.FullPaperFile.FileName);
        //        fullPaperURL = _objectStorageSettings.Value.EndPoint + await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.fullpaperfile.ToString(),uniqueFileName,stream,request.FullPaperFile.ContentType);
        //    }
        //    var pendingStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync("Pending");
        //    var fullPaperObject = request.toModel(fullPaperURL, pendingStatus.ReviewStatusId);
        //    await _unitOfWork.BeginTransactionAsync();
        //    try {
        //        await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaperObject);
        //        PaperBase.FullPaperId = fullPaperObject.FullPaperId;
        //        await _unitOfWork.CommitAsync();
        //        return fullPaperObject.toResponse();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackAsync(); 
        //        throw;
        //    }

        //}
        public async Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId)
        {
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            if (pendingReviewStatus == null || currentFullPaperPhase == null)
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
                throw new BadRequestException($"Không thể gửi full paper vì paper đang không trong trạng thái full paper");
            }
            if (paper.FullPaperId != null)
            {
                throw new BadRequestException($"Full paper file đã có trong hệ thống");
            }
            if (paper.PresenterId != userId)
            {
                throw new BadRequestException($"Bạn không phải chủ sỡ hữu bài báo này");
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
            }
            var fullPaper = new FullPaper()
            {
                FullPaperId = Guid.NewGuid().ToString(),
                FullPaperUrl = fullPaperFileUrl,
                ReviewStatusId = pendingReviewStatus.ReviewStatusId,
            };
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                result = result + await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaper);
                paper.FullPaperId = fullPaper.FullPaperId;
                result = result +  await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            return result;

        }


        public async Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId)
        {
            if (request.ReviewStatus == ReviewStatusEnum.Pending)
            {
                throw new BadRequestException("Không thể chuyển trạng thái full paper status Pending.");
            }
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var rejectedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
            var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
            var reviseStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Revise.GetDescription());


            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            var revisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobal = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

            if (pendingReviewStatus == null || rejectedReviewStatus==null|| acceptedReviewStatus==null|| reviseStatus==null|| currentFullPaperPhase == null || cameraReadyPhase==null|| revisePhase==null|| pendingGlobal==null)
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
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.ReviewStatus)
                {
                    case ReviewStatusEnum.Accepted:

                        fullPaper.ReviewStatusId = acceptedReviewStatus.ReviewStatusId;
                        paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;

                        break;
                    case ReviewStatusEnum.Rejected:


                        fullPaper.ReviewStatusId = rejectedReviewStatus.ReviewStatusId;
                        break;
                    case ReviewStatusEnum.Revise:



                        fullPaper.ReviewStatusId = reviseStatus.ReviewStatusId;
                        paper.PaperPhaseId = revisePhase.PaperPhaseId;


                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }
                result += await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);

                await _unitOfWork.CommitAsync();
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            
            return result;


        }

        public async Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId)
        {
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (currentRevisePhase == null || pendingGlobalStatus ==null)
            {
                throw new NotFoundException($"Không thể tìm thấy trạng thái tương ứng trong hệ thống");
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
           
            
            if (paper.PresenterId != userId)
            {
                throw new ConfRadarAuthenticationException("Bạn không có quyền nộp revision cho bài báo này");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            string revisionDeadlineId = string.Empty; 
            var researchConferencePhasesFound = paper.Conference.ResearchConferencePhases;
            foreach(var phase in researchConferencePhasesFound)
            {
                if (phase.ReviseStartDate != null && phase.ReviseEndDate != null && dateNow >= phase.ReviseStartDate && dateNow <= phase.ReviseEndDate)
                {
                    foreach(var deadline in phase.RevisionRoundDeadlines)
                    {
                        if (deadline.EndDate != null && dateNow <= deadline.EndDate)
                        {
                            revisionDeadlineId = deadline.RevisionRoundDeadlineId;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(revisionDeadlineId))
                    {
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(revisionDeadlineId))
            {
                throw new NotFoundException($"Không thể tìm thấy bất cứ hạn chót revision trong hệ thống vui lòng liên hệ conference organizer để xử lí");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                RevisionPaper revisionPaper;

                if (paper.RevisionPaperId == null)
                {
                    revisionPaper = new RevisionPaper()
                    {
                        RevisionPaperId = Guid.NewGuid().ToString(),
                        RevisionRound = 1,
                        GlobalStatusId = pendingGlobalStatus.GlobalStatusId
                    };
                    await _unitOfWork.RevisionPaperRepository.CreateRevisionPaperAsync(revisionPaper);

                    paper.RevisionPaperId = revisionPaper.RevisionPaperId;
                    await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                }
                else
                {
                    revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
                    if (revisionPaper == null)
                    {
                        throw new BadRequestException($"Revision paper id {paper.RevisionPaperId} không tìm thấy trong hệ thống");
                    }
                    revisionPaper.RevisionRound = revisionPaper.RevisionRound  + 1;
                }

                var totalRevisionRoundAllowed = paper.Conference!.ResearchConferenceDetail!.RevisionAttemptAllowed;
                if (revisionPaper.RevisionRound > totalRevisionRoundAllowed) 
                {
                    throw new BadRequestException($"Không thể nộp thêm paper submission vì đã quá {totalRevisionRoundAllowed} lần, vui lòng chờ phán quyết từ head reviewer!");
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
                    RevisionPaperId = revisionPaper.RevisionPaperId,
                    RevisionDeadlineRoundId = revisionDeadlineId,
                    RevisionPaperUrl = revisionFileUrl,
                };

                var result1 = await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
                var result2 = await _unitOfWork.RevisionPaperSubmissionRepository.CreateRevisionPaperSubmissionAsync(revisionPaperSubmissionObj);

                await _unitOfWork.CommitAsync();
                return result1 + result2;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
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

        public async Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId)
        {
            if (request.GlobalStatus == GlobalStatusEnum.Pending)
            {
                throw new BadRequestException($"Không thể chuyển trạng thái pending");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper  id {request.PaperId} trong hệ thống");
            }
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không thể gửi review vì paper đang không trong trạng thái revise");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(request.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Không tìm bạn với id {userId} được chấm bài {request.PaperId} trong hệ thống");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper {request.RevisionPaperId} trong hệ thống");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm thấy revision paper {request.RevisionPaperId} tương ứng với paper trong hệ thống");
            }
            if (revisionPaper.GlobalStatusId == acceptedGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision này đã được chấp nhận nên bạn không thể gửi review");
            }
            string revisionReviewUrl = string.Empty;
            if (request.FeedbackMaterialFile != null)
            {
                using var stream = request.FeedbackMaterialFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.FeedbackMaterialFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.revisionpaperreviewfile.ToString(), uniqueFileName, stream, request.FeedbackMaterialFile.ContentType);
                revisionReviewUrl = baseUri + objectStorageFileUrl;
            }
            string globalStatusId = string.Empty;
            if (request.GlobalStatus == GlobalStatusEnum.Accepted)
            {
                globalStatusId = acceptedGlobalStatus.GlobalStatusId;
            }
            else
            {
                var rejectGlobalStautus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                globalStatusId = rejectGlobalStautus.GlobalStatusId;
            }
            var revisionPaperReviewObj = new RevisionPaperReview()
            {
                RevisionPaperReviewId = Guid.NewGuid().ToString(),
                GlobalStatusId = globalStatusId,
                Note = request.Note,
                CreatedAt = ExtensionHelper.GetVietnamTime(),
                FeedbackToAuthor = request.FeedbackToAuthor,
                FeedbackMaterialUrl = revisionReviewUrl,
                ReviewerId = userId,
                RevisionPaperId = request.RevisionPaperId,
            };
            return await _unitOfWork.RevisionPaperReviewRepository.CreateRevisionPaperReviewAsync(revisionPaperReviewObj);
        }

        public async Task<int> DecideReviseStatus(UpdateRevisionStatusRequest request, string userId)
        {
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectGlobalStautus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

            var cameraReadyPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            if (request.GlobalStatus == GlobalStatusEnum.Pending)
            {
                throw new BadRequestException("Không thể chuyển trạng thái pending cho giai đoạn revise");
            }
            if (currentRevisePhase == null || pendingGlobalStatus == null || acceptedGlobalStatus == null || cameraReadyPaperPhase ==null|| rejectGlobalStautus==null)
            {
                throw new NotFoundException("Không tìm thấy các trạng thái trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy  paper {request.PaperId} trong hệ thống");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper đang không ở trong trạng thái revise");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy  revision paper {request.RevisionPaperId} trong hệ thống");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Paper {request.PaperId} không thuộc revision paper {request.RevisionPaperId}");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(request.PaperId, userId);
            if (paperReviewer == null )
            {
                throw new NotFoundException($"Bạn không có quyền hạn để quyết định bài báo này");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new BadRequestException($"Bạn không phải là head reviewer để quyết định status của bài báo này");
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        revisionPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        paper.PaperPhaseId = cameraReadyPaperPhase.PaperPhaseId;
                        break;

                    case GlobalStatusEnum.Rejected:
                        revisionPaper.GlobalStatusId = rejectGlobalStautus.GlobalStatusId;
                        break;

                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }

                var result1 = await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
                var result2 = await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);

                await _unitOfWork.CommitAsync();
                return result1 + result2;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<RevisionPaperReviewResponse>> ListRevisionPaperReview(ListRevisionPaperReviewRequest request, string userId)
        {
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy  paper {request.PaperId} trong hệ thống");
            }
            
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(request.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Bạn không có quyền hạn để truy cập bài báo này");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm thấy revision paper id {request.RevisionPaperId} trong paper {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException($"Bạn không phải là head reviewer để xem danh sách này");
            }
            var listRevisionPaperReview = await _unitOfWork.RevisionPaperReviewRepository.GetRevisionPaperReviewByRevisionPaperIdAsync(request.RevisionPaperId);
            var listRevisionPaperReviewResponse = listRevisionPaperReview.Select(x => new RevisionPaperReviewResponse
            {
                RevisionPaperReviewId = x.RevisionPaperReviewId,
                GlobalStatusId = x.GlobalStatusId,
                GlobalStatusName = x.GlobalStatus?.Name,
                Note = x.Note,
                CreatedAt = x.CreatedAt,
                FeedbackToAuthor = x.FeedbackToAuthor,
                FeedbackMaterialUrl = x.FeedbackMaterialUrl,
                ReviewerId = x.ReviewerId,
                ReviewerName = x.Reviewer?.FullName,
                ReviewerAvatarUrl = x.Reviewer?.AvatarUrl,
                RevisionPaperId = x.RevisionPaperId,
            }).ToList();
            return listRevisionPaperReviewResponse;
        }

        public async Task<string> CreateCameraReady(CreateCameraReadyRequest request, string userId)
        {
            // Validate that the paper exists
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper with ID {request.PaperId} does not exist.");
            }

            // Check if paper already has a camera ready
            if (!string.IsNullOrEmpty(paper.CameraReadyId))
            {
                throw new BadRequestException($"Paper with ID {request.PaperId} already has a camera ready record.");
            }

            // Validate that the user is the presenter of the paper
            if (paper.PresenterId != userId)
            {
                throw new BadRequestException("You are not authorized to create camera ready for this paper.");
            }

            // Validation: Paper must have either:
            // 1. RevisionPaper with GlobalStatus = "Accepted", OR
            // 2. FullPaper with ReviewStatus = "Accepted"
            bool isValidPaper = false;

            if (!string.IsNullOrEmpty(paper.RevisionPaperId))
            {
                // Check if RevisionPaper exists and has GlobalStatus = "Accepted"
                var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
                if (revisionPaper != null && revisionPaper.GlobalStatus != null)
                {
                    var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    if (revisionPaper.GlobalStatusId == acceptedGlobalStatus.GlobalStatusId)
                    {
                        isValidPaper = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(paper.FullPaperId))
            {
                // Check if FullPaper exists and has ReviewStatus = "Accepted"
                var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId);
                if (fullPaper != null && fullPaper.ReviewStatus != null)
                {
                    var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
                    if (fullPaper.ReviewStatusId == acceptedReviewStatus.ReviewStatusId)
                    {
                        isValidPaper = true;
                    }
                }
            }

            if (!isValidPaper)
            {
                throw new BadRequestException("Paper must have either an accepted revision paper or an accepted full paper to create camera ready.");
            }

            // Upload camera ready file
            string cameraReadyFileUrl = string.Empty;
            if (request.CameraReadyFile != null)
            {
                if (request.CameraReadyFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }

                using var stream = request.CameraReadyFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.CameraReadyFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.camerareadyfile.ToString(), uniqueFileName, stream, request.CameraReadyFile.ContentType);
                cameraReadyFileUrl = baseUri + objectStorageFileUrl;
            }

            // Get Pending GlobalStatus
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

            // Create CameraReady record
            var cameraReady = new CameraReady
            {
                CameraReadyId = Guid.NewGuid().ToString(),
                GlobalStatusId = pendingGlobalStatus.GlobalStatusId,
                CameraReadyUrl = cameraReadyFileUrl,
            };

            // Save CameraReady
            await _unitOfWork.CameraReadyRepository.CreateCameraReadyAsync(cameraReady);

            // Update Paper with CameraReadyId
            paper.CameraReadyId = cameraReady.CameraReadyId;
            
            // Update paper phase to CameraReady
            var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            if (cameraReadyPhase != null)
            {
                paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;
            }

            await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);

            return cameraReady.CameraReadyId;
        }

        public async Task<int> UpdateCameraReady(UpdateCameraReadyRequest request, string userId)
        {
            // Validate that the camera ready exists
            var cameraReady = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(request.CameraReadyId);
            if (cameraReady == null)
            {
                throw new BadRequestException($"Camera ready with ID {request.CameraReadyId} does not exist.");
            }

            // Validate that the camera ready is in "Pending" status
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (cameraReady.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Camera ready must be in pending status to be updated.");
            }

            // Find the paper associated with this camera ready
            var paper = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(request.CameraReadyId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper associated with camera ready ID {request.CameraReadyId} does not exist.");
            }

            // Validate that the user is a head reviewer of the paper
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(paper.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new BadRequestException("You are not a reviewer of this paper.");
            }

            if (paperReviewer.IsHeadReviewer != true)
            {
                throw new BadRequestException("Only head reviewers can update camera ready.");
            }

            // Upload new camera ready file
            if (request.CameraReadyFile != null)
            {
                if (request.CameraReadyFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }

                using var stream = request.CameraReadyFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.CameraReadyFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.camerareadyfile.ToString(), uniqueFileName, stream, request.CameraReadyFile.ContentType);
                cameraReady.CameraReadyUrl = baseUri + objectStorageFileUrl;
            }

            // Update CameraReady
            return await _unitOfWork.CameraReadyRepository.UpdateCameraReadyAsync(cameraReady);
        }

        public async Task<string> SubmitReviewForFullPaper(CreateFullPaperReviewRequest request, string userId)
        {
            // Validate that the user exists and has reviewer role
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new BadRequestException($"User with ID {userId} does not exist.");
            }

            // Check if user is a reviewer (either Local Reviewer or External Reviewer)
            var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Local Reviewer");
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("External Reviewer");

            if (localReviewerRole == null || externalReviewerRole == null)
            {
                throw new BadRequestException("Reviewer roles do not exist in the system.");
            }

            var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);
            var hasReviewerRole = userRoles.Any(ur => ur.RoleId == localReviewerRole.RoleId || ur.RoleId == externalReviewerRole.RoleId);

            if (!hasReviewerRole)
            {
                throw new BadRequestException("User must have Local Reviewer or External Reviewer role to submit a review.");
            }

            // Validate that the full paper exists
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper with ID {request.FullPaperId} does not exist.");
            }

            // Validate that the user is assigned as a reviewer to this paper
            var paper = await _unitOfWork.PaperRepository.GetPaperByFullPaperIdAsync(request.FullPaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper associated with full paper ID {request.FullPaperId} does not exist.");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(paper.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new BadRequestException("You are not assigned as a reviewer to this paper.");
            }

            // Check if the user has already submitted a review for this full paper
            var existingReview = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(request.FullPaperId, userId);
            if (existingReview != null)
            {
                throw new BadRequestException("You have already submitted a review for this full paper.");
            }

            // Validate that the full paper is in "Pending" review status
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            if (pendingReviewStatus == null)
            {
                throw new BadRequestException("Pending review status does not exist in the system.");
            }

            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException("Full paper must be in Pending status to submit a review.");
            }

            // Upload feedback material file if provided
            string feedbackMaterialUrl = string.Empty;
            if (request.FeedbackMaterialFile != null)
            {
                if (request.FeedbackMaterialFile.ContentType == null)
                {
                    throw new BadRequestException("Content type is null");
                }

                using var stream = request.FeedbackMaterialFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.FeedbackMaterialFile.FileName);
                var baseUri = _objectStorageSettings.Value.EndPoint;
                var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.feedbackmaterial.ToString(), uniqueFileName, stream, request.FeedbackMaterialFile.ContentType);
                feedbackMaterialUrl = baseUri + objectStorageFileUrl;
            }

            // Create the full paper review
            var fullPaperReview = new FullPaperReview
            {
                FullPaperReviewId = Guid.NewGuid().ToString(),
                FullPaperId = request.FullPaperId,
                ReviewerId = userId,
                ReviewStatusId = pendingReviewStatus.ReviewStatusId,
                Note = request.Note,
                FeedbackToAuthor = request.FeedbackToAuthor,
                FeedbackMaterialUrl = feedbackMaterialUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FullPaperReviewRepository.CreateFullPaperReviewAsync(fullPaperReview);

            return fullPaperReview.FullPaperReviewId;
        }

        public async Task<List<FullPaperReviewResponse>> GetFullPaperReviewsByFullPaperId(string fullPaperId)
        {
            // Validate that the full paper exists
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(fullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper with ID {fullPaperId} does not exist.");
            }

            // Get all reviews for this full paper
            var fullPaperReviews = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewsByFullPaperIdAsync(fullPaperId);

            // Convert to response objects
            var fullPaperReviewResponses = fullPaperReviews.Select(review => new FullPaperReviewResponse
            {
                FullPaperReviewId = review.FullPaperReviewId,
                GlobalStatusId = review.ReviewStatusId,
                GlobalStatusName = review.ReviewStatus?.Name,
                Note = review.Note,
                CreatedAt = review.CreatedAt,
                FeedbackToAuthor = review.FeedbackToAuthor,
                FeedbackMaterialUrl = review.FeedbackMaterialUrl,
                ReviewerId = review.ReviewerId,
                ReviewerName = review.Reviewer?.FullName,
                ReviewerAvatarUrl = review.Reviewer?.AvatarUrl,
                FullPaperId = review.FullPaperId
            }).ToList();

            return fullPaperReviewResponses;
        }

        //public async Task<int> SubmitFullPaperReviewStatus(UpdateFullPaperReviewStatusRequest request, string userId)
        //{
        //    // Validate that the full paper review exists
        //    var fullPaperReview = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewByIdAsync(request.FullPaperReviewId);
        //    if (fullPaperReview == null)
        //    {
        //        throw new BadRequestException($"Full paper review with ID {request.FullPaperReviewId} does not exist.");
        //    }

        //    // Validate that the full paper review is in "Pending" status
        //    var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
        //    if (pendingReviewStatus == null)
        //    {
        //        throw new BadRequestException("Pending review status does not exist in the system.");
        //    }

        //    if (fullPaperReview.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
        //    {
        //        throw new BadRequestException("Full paper review must be in Pending status to update its status.");
        //    }

        //    // Validate that the user is a head reviewer for the paper associated with this full paper
        //    var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(fullPaperReview.FullPaperId);
        //    if (fullPaper == null)
        //    {
        //        throw new BadRequestException($"Full paper with ID {fullPaperReview.FullPaperId} does not exist.");
        //    }

        //    var paper = await _unitOfWork.PaperRepository.GetPaperByFullPaperIdAsync(fullPaper.FullPaperId);
        //    if (paper == null)
        //    {
        //        throw new BadRequestException($"Paper associated with full paper ID {fullPaper.FullPaperId} does not exist.");
        //    }

        //    var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(paper.PaperId, userId);
        //    if (paperReviewer == null)
        //    {
        //        throw new BadRequestException("You are not assigned as a reviewer to this paper.");
        //    }

        //    if (paperReviewer.IsHeadReviewer != true)
        //    {
        //        throw new BadRequestException("Only head reviewers can decide the status of full paper reviews.");
        //    }

        //    // Update the review status based on the request
        //    ReviewStatus? newReviewStatus = null;
        //    switch (request.Statusreview.Name)
        //    {
        //        case "Accepted":
        //            newReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
        //            break;
        //        case "Rejected":
        //            newReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
        //            break;
        //        case "Revise":
        //            newReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Revise.GetDescription());
        //            break;
        //        default:
        //            throw new BadRequestException("Invalid global status for full paper review.");
        //    }

        //    if (newReviewStatus == null)
        //    {
        //        throw new BadRequestException($"{request.Statusreview.Name} review status does not exist in the system.");
        //    }

        //    fullPaperReview.ReviewStatusId = newReviewStatus.ReviewStatusId;

        //    return await _unitOfWork.FullPaperReviewRepository.UpdateFullPaperReviewAsync(fullPaperReview);
        //}

        public async Task<int> DecideCameraReadyStatus(UpdateCameraReadyStatusRequest request, string userId)
        {
            // Validate that the camera ready exists
            var cameraReady = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(request.CameraReadyId);
            if (cameraReady == null)
            {
                throw new BadRequestException($"Camera ready with ID {request.CameraReadyId} does not exist.");
            }

            // Validate that the camera ready is in "Pending" status
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingGlobalStatus == null)
            {
                throw new BadRequestException("Pending global status does not exist in the system.");
            }

            if (cameraReady.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Camera ready must be in Pending status to update its status.");
            }

            // Validate that the user is a head reviewer for the paper associated with this camera ready
            var paper = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(request.CameraReadyId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper associated with camera ready ID {request.CameraReadyId} does not exist.");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(paper.PaperId, userId);
            if (paperReviewer == null)
            {
                throw new BadRequestException("You are not assigned as a reviewer to this paper.");
            }

            if (paperReviewer.IsHeadReviewer != true)
            {
                throw new BadRequestException("Only head reviewers can decide the status of camera ready submissions.");
            }

            // Update the camera ready status based on the request
            GlobalStatus? newGlobalStatus = null;
            switch (request.GlobalStatus)
            {
                case GlobalStatusEnum.Accepted:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    break;
                case GlobalStatusEnum.Rejected:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                    break;
                case GlobalStatusEnum.Pending:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
                    break;
                default:
                    throw new BadRequestException("Invalid global status for camera ready.");
            }

            if (newGlobalStatus == null)
            {
                throw new BadRequestException($"{request.GlobalStatus.GetDescription()} global status does not exist in the system.");
            }

            cameraReady.GlobalStatusId = newGlobalStatus.GlobalStatusId;

            return await _unitOfWork.CameraReadyRepository.UpdateCameraReadyAsync(cameraReady);
        }

       
        public async Task <List<PapersAssignedToReviewerResponse>> GetAllAssignedPapersToAReviewer(string userId, string conferenceId)
        {
            // Use the new repository method to get paper reviewers for user and conference
            var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, conferenceId);
            
            var assignedPapers = paperReviewers
                .Where(pr => pr.Paper != null)
                .Select(pr => pr.Paper)
                .ToList();
            
            var result = assignedPapers.Select(s => new PapersAssignedToReviewerResponse
            {
                Paper = s,
                phaseName = s.PaperPhase?.PhaseName
            }).ToList();
            
            return result;
        }

        public async Task<List<Paper>> GetSubmittedPaper(string userId)
        {
            // Use the new repository method to get papers by user ID in a single query
            var submittedPapers = await _unitOfWork.PaperAuthorRepository.GetPapersByUserIdAsync(userId);
            
            return submittedPapers;
        }

        public async Task<PaperDetailReponse> getPaperDetail(string paperId)
        {
            // Use the repository method to get paper with its phase
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdWithPhaseAsync(paperId);
            
            if (paper == null) 
            {
                throw new Exception($"Không tìm thấy paper với id {paperId}");
            }
            
            // Create a minimal PaperPhase object to avoid circular references during serialization
            // The original PaperPhase entity might have its Papers collection loaded, causing cycles
            var currentPhase = paper.PaperPhase != null ? new PaperPhase
            {
                PaperPhaseId = paper.PaperPhase.PaperPhaseId,
                PhaseName = paper.PaperPhase.PhaseName,
                // Papers collection is intentionally left empty to avoid cycles
            } : null;
            
            return new PaperDetailReponse
            {
                PaperId = paperId,
                currentPhase = currentPhase, // Use the safe version to avoid cycles
                Abstract = paper.AbstractId != null ? await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId): null,
                FullPaper = paper.FullPaperId != null ? await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId) : null,
                RevisionPaper = paper.RevisionPaperId != null ? await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId) : null,
                CameraReady = paper.CameraReadyId != null ? await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(paper.CameraReadyId) : null,
            };
        }


        public async Task<List<PendingAbstractResponse>> GetListPendingAbstract()
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái trong hệ thống");
            }
            var listAbstract = await _unitOfWork.AbstractRepository.GetAllPendingAbstractsAsync(pendingGlobalStatus.GlobalStatusId);
            return listAbstract;
        }

        public async Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases()
        {
           return await _unitOfWork.PaperPhaseRepository.GetAllPaperPhasesAsync();
        }

        public async Task<List<PaperDetailResponseDTO>> GetListAllPaper()
        {
            var papers = await _unitOfWork.PaperRepository.GetAllPapersAsync();
            var result = new List<PaperDetailResponseDTO>();

            foreach (var p in papers)
            {
                Abstract abstractEntity = null;
                if (p.AbstractId != null)
                {
                    abstractEntity = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(p.AbstractId);
                }

                FullPaper fullPaperEntity = null;
                if (p.FullPaperId != null)
                {
                    fullPaperEntity = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(p.FullPaperId);
                }

                RevisionPaper revisionEntity = null;
                if (p.RevisionPaperId != null)
                {
                    revisionEntity = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(p.RevisionPaperId);
                }

                CameraReady cameraReadyEntity = null;
                if (p.CameraReadyId != null)
                {
                    cameraReadyEntity = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(p.CameraReadyId);
                }

                var paperDto = new PaperDetailResponseDTO
                {
                    PaperId = p.PaperId,
                    currentPhase = new PaperPhaseResponseDTO
                    {
                        PaperPhaseId = p.PaperPhase?.PaperPhaseId ?? "",
                        PhaseName = p.PaperPhase?.PhaseName
                    },
                    Abstract = abstractEntity != null ? new AbstractResponseDTO
                    {
                        AbstractId = abstractEntity.AbstractId,
                        GlobalStatusId = abstractEntity.GlobalStatusId,
                        GlobalStatusName = abstractEntity.GlobalStatus?.Name,
                        AbstractUrl = abstractEntity.AbstractUrl
                    } : null,
                    FullPaper = fullPaperEntity != null ? new FullPaperResponseDTO
                    {
                        FullPaperId = fullPaperEntity.FullPaperId,
                        ReviewStatusId = fullPaperEntity.ReviewStatusId,
                        ReviewStatusName = fullPaperEntity.ReviewStatus?.Name,
                        FullPaperUrl = fullPaperEntity.FullPaperUrl
                    } : null,
                    RevisionPaper = revisionEntity != null ? new RevisionPaperResponseDTO
                    {
                        RevisionPaperId = revisionEntity.RevisionPaperId,
                        RevisionRound = revisionEntity.RevisionRound,
                        GlobalStatusId = revisionEntity.GlobalStatusId,
                        GlobalStatusName = revisionEntity.GlobalStatus?.Name
                    } : null,
                    CameraReady = cameraReadyEntity != null ? new CameraReadyResponseDTO
                    {
                        CameraReadyId = cameraReadyEntity.CameraReadyId,
                        GlobalStatusId = cameraReadyEntity.GlobalStatusId,
                        GlobalStatusName = cameraReadyEntity.GlobalStatus?.Name,
                        CameraReadyUrl = cameraReadyEntity.CameraReadyUrl
                    } : null
                };

                result.Add(paperDto);
            }
            return result;
        }
    }
}
