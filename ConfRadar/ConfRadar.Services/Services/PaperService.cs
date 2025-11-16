using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;
using ConfRadar.Services.DTOs.FullPaperReview;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.RevisionPaper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using ConfRadar.Shared.DTO.Abstract;
using ConfRadar.Shared.DTO.Paper;
using ConfRadar.Shared.DTO.WaitList;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        #region n?p paper

        Task<int> SubmitAbstract(CreateAbstractRequest request, string userId);
        Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId);
        Task<string> SubmitReviewForFullPaper(CreateFullPaperReviewRequest request, string userId);

        Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId);
        Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId);
        Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request, string userId);
        Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId);
        Task<string> CreateCameraReady(CreateCameraReadyRequest request, string userId);

        #endregion



        #region update paper
        Task<int> UpdateAbstract(UpdateAbstractRequest request, string userId);
        Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId);
        Task<int> UpdateRevisionPaperSubmission(UpdateRevisionPaperRevisionSubmissionRequest request, string userId);
        Task<int> UpdateCameraReady(UpdateCameraReadyRequest request, string userId);

        #endregion


        #region quy?t d?nh
        Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId);
        Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId);
        Task<int> DecideReviseStatus(UpdateRevisionStatusRequest request, string userId);
        Task<int> DecideCameraReadyStatus(UpdateCameraReadyStatusRequest request, string userId);
        #endregion


        #region get detail
        Task<List<PendingAbstractResponse>> GetListPendingAbstract(string? confId);

        //Task<FullPaperResponse> SubmitFullPaper (CreateFullPaperRequest request, string userId);
        //cho head reviewer quy?t d?nh cu?i cùng

        //Task<int> (UpdateFullPaperReviewStatusRequest request, string userId);

        //g?i review  cho head reviewer xem
        Task<List<FullPaperReviewResponse>> GetFullPaperReviewsByFullPaperId(string fullPaperId);


        Task<List<RevisionPaperReviewResponse>> ListRevisionPaperReview(ListRevisionPaperReviewRequest request, string userId);
        Task<List<PapersAssignedToReviewerResponse>> GetAllAssignedPapersToAReviewer(string userId, string conferenceId);

        Task<List<Paper>> GetSubmittedPaper(string userId, string? confId);
        Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId);

        Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases();
        Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersByReviewerId(string userId, string? confId);
        //Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersGroupedByConference(string userId, string? confId);
        Task<List<CameraReadyDtoDetail>> ListPendingCameraReady();
        Task<List<FullPaperDtoDetail>> ListPendingfullpaper();


        Task<List<PaperDetailResponseDTO>> GetListAllPaper();
        Task<List<UnAssignAbstractResponse>> GetUnassignAbstractList();
        Task<PaperDetailForReviewerResponse> GetPaperDetailForReviewer(string paperId, string userId);

        Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId);
        Task<LeaveWaitListResponse> LeaveWaitList(string userId, string conferenceId);
        Task<AddWaitListResponse> AddWaitList(string userId, string conferenceId);
        #endregion



    }
    public class PaperService : IPaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMomoService _momoService;
        private readonly ITokenService _tokenService;
        private readonly IOptions<ObjectStorageSettings> _objectStorageSettings;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITicketService _ticketService;
        private readonly ITimeProviderService _timeProviderService;
        public PaperService(IUnitOfWork unitOfWork, IMomoService momoService, ITokenService tokenService, IOptions<ObjectStorageSettings> objectStorageSettings, IObjectStorageFileService objectStorageFileService, ITicketService ticketService, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _momoService = momoService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
            _ticketService = ticketService;
            _timeProviderService = timeProviderService;
        }

        public async Task<int> SubmitAbstract(CreateAbstractRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var paperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (paperPhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException($"Không tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y paper v?i id {request.PaperId} trong h? th?ng");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y các giai do?n cho h?i ngh? nghiên c?u {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Giai do?n n?p abstract di?n ra t? {activeCurrentPhase.RegistrationStartDate} d?n {activeCurrentPhase.RegistrationEndDate}");
            }

            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hi?n t?i không dang trong quá trình g?i abstract");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);

            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }

            var submitterReviewContracts = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            if (request.CoAuthorId != null && request.CoAuthorId.Count > 0)
            {
                foreach (var coauthorId in request.CoAuthorId)
                {
                    if (coauthorId == userId)
                    {
                        throw new BadRequestException("B?n không th? thêm chính mình làm co-author.");
                    }

                    bool isCoauthorReviewerInPaperReviewer = submitterReviewContracts.Any(pr => pr.UserId == coauthorId);
                    var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(coauthorId, paper.Conference!.ConferenceId);
                    if (reviewerContractFound != null)
                    {
                        if (reviewerContractFound.IsActive == true)
                        {
                            throw new BadRequestException($"Co author v?i id {coauthorId} hi?n dang có h?p d?ng review");
                        }
                    }
                    if (isCoauthorReviewerInPaperReviewer == true)
                    {
                        throw new BadRequestException($"Ngu?i dùng {coauthorId} dang là reviewer c?a bài báo này, không th? thêm làm co-author.");
                    }
                }
            }


            if (paper.AbstractId != null)
            {
                throw new BadRequestException("Paper này dã có abstract du?c n?p r?i");
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
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                Description = request.Description,
                Title = request.Title,
                ReviewAt = null,
            };
            paper.AbstractId = abstractObj.AbstractId;


            List<PaperAuthor> paperAuthorList = new List<PaperAuthor>();
            if (request.CoAuthorId != null && request.CoAuthorId.Count > 0)
            {
                foreach (var coAuthor in request.CoAuthorId)
                {
                    var paperAuthorObj = new PaperAuthor()
                    {
                        IsPresenter = false,
                        UserId = coAuthor,
                        PaperId = request.PaperId,
                        IsRootAuthor = false,

                    };
                    paperAuthorList.Add(paperAuthorObj);
                }
            }



            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int finalResult = 0;
                finalResult += await _unitOfWork.AbstractRepository.CreateAbstractAsync(abstractObj);
                finalResult += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                if (paperAuthorList.Count > 0)
                {
                    finalResult += await _unitOfWork.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(paperAuthorList);
                }
                await _unitOfWork.CommitAsync();
                return finalResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId)
        {
            if (request.GlobalStatus.Equals(GlobalStatusEnum.Pending))
            {
                throw new BadRequestException($"Không th? truy?n tr?ng thái pending cho abstract");
            }
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

            var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription());
            var abstractPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (abstractPaperPhase == null || pendingGlobalStatus == null || rejectedGlobalStatus == null || acceptedGlobalStatus == null || fullPaperPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (basePaper == null)
            {
                throw new NotFoundException($"Không tìm th?y paper v?i id {request.PaperId} trong h? th?ng");
            }
            var activeCurrentPhase = basePaper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {basePaper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Ph?i trong kho?ng Registration Date d? có th? c?p nh?t tr?ng thái abstract này: trong registation start {activeCurrentPhase.RegistrationStartDate.ToString()} và registration end {activeCurrentPhase.RegistrationEndDate.ToString()}");
            }
            if (basePaper.PaperPhaseId != abstractPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hi?n t?i không dang trong quá trình quy?t d?nh abstract");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(request.AbstractId);
            if (abstractPaper == null)
            {
                throw new NotFoundException($"Không tìm th?y abstract paper v?i id {request.AbstractId} trong h? th?ng");
            }
            if (abstractPaper.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Abstract hi?n t?i không dang trong tr?ng thái pending, vui lòng th? l?i sau");
            }
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        abstractPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        abstractPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        basePaper.PaperPhaseId = fullPaperPhase.PaperPhaseId;
                        break;
                    case GlobalStatusEnum.Rejected:
                        abstractPaper.GlobalStatusId = rejectedGlobalStatus.GlobalStatusId;
                        abstractPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        var rootAuthor = basePaper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
                        var ticket = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(rootAuthor!.UserId, basePaper.ConferenceId!);
                        await _ticketService.RefundAuthorCloneFunction(rootAuthor!.UserId, ticket.TicketId, "Abstract b?n dã b? t? ch?i");


                        break;
                    default:
                        throw new BadRequestException("Tr?ng thái không kh? d?ng");
                }
                result += await _unitOfWork.AbstractRepository.UpdateAbstractAsync(abstractPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(basePaper);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            return result;
        }



        //public async Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId)
        //{
        //    if (request.PaperId == null) throw new Exception("C?n có paperid d? n?p fullpaper");
        //    var PaperBase = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
        //    if (PaperBase == null) throw new Exception($"Không tìm th?y paper v?i id{request.PaperId}");
        //    string fullPaperURL = string.Empty;
        //    if(request.FullPaperFile != null)
        //    {
        //        if (request.FullPaperFile.ContentType == null) throw new Exception("Không có d? li?u file d?u vào d? n?p");
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
                throw new NotFoundException($"Không th? tìm th?y các tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không th? tìm th?y paper id: {request.PaperId} cho user {userId} hi?n t?i");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai do?n n?p full paper di?n ra t? {activeCurrentPhase.FullPaperStartDate} d?n {activeCurrentPhase.FullPaperEndDate}");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không th? g?i full paper vì paper dang không trong tr?ng thái full paper");
            }
            if (paper.FullPaperId != null)
            {
                throw new BadRequestException($"Full paper file dã có trong h? th?ng");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
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
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                ReviewAt = null,
                Description = request.Description,
                Title = request.Title,
            };
            paper.FullPaperId = fullPaper.FullPaperId;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = 0;
                result += await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


        public async Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId)
        {
            if (request.ReviewStatus == ReviewStatusEnum.Pending)
            {
                throw new BadRequestException("Không th? chuy?n tr?ng thái full paper status Pending.");
            }
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var rejectedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
            var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
            var reviseStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Revise.GetDescription());


            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            var revisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobal = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

            if (pendingReviewStatus == null || rejectedReviewStatus == null || acceptedReviewStatus == null || reviseStatus == null || currentFullPaperPhase == null || cameraReadyPhase == null || revisePhase == null || pendingGlobal == null)
            {
                throw new NotFoundException($"Không th? tìm th?y các tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không tìm th?y paper v?i id {request.PaperId}.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviewStartDate || dateNow > activeCurrentPhase.ReviewEndDate)
            {
                throw new BadRequestException($"Giai do?n review cho bài báo này di?n ra t? {activeCurrentPhase.ReviewStartDate} d?n {activeCurrentPhase.ReviewEndDate}");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper v?i id {request.FullPaperId} không tìm th?y");
            }
            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper v?i id ph?i là tr?ng thái (Pending) d? du?c c?p nh?t");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper ph?i dang trong full paper phase d? có th? c?p nh?t tr?ng thái");
            }
            var paperReviewerList = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            if (paperReviewerList == null || paperReviewerList.Count <= 0)
            {
                throw new NotFoundException($"Không tìm th?y các danh sách gán reviewer cho bài báo này");
            }
            var headPaperReviewer = paperReviewerList.FirstOrDefault(x => x.IsHeadReviewer == true && x.UserId == userId);
            if (headPaperReviewer == null)
            {
                throw new NotFoundException($"Không tìm th?y b?n là head reviewer trong danh sách gán reviewer.");
            }
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.ReviewStatus)
                {
                    case ReviewStatusEnum.Accepted:

                        fullPaper.ReviewStatusId = acceptedReviewStatus.ReviewStatusId;
                        fullPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;

                        break;
                    case ReviewStatusEnum.Rejected:


                        fullPaper.ReviewStatusId = rejectedReviewStatus.ReviewStatusId;
                        fullPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        var rootAuthor = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
                        var ticket = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(rootAuthor!.UserId, paper.ConferenceId!);
                        await _ticketService.RefundAuthorCloneFunction(rootAuthor!.UserId, ticket.TicketId, "Full paper b?n dã b? t? ch?i");
                        break;
                    case ReviewStatusEnum.Revise:



                        fullPaper.ReviewStatusId = reviseStatus.ReviewStatusId;
                        fullPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        paper.PaperPhaseId = revisePhase.PaperPhaseId;


                        break;
                    default:
                        throw new BadRequestException("Tr?ng thái không kh? d?ng");
                }
                result += await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
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
            if (currentRevisePhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException($"Không th? tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm th?y trong h? th?ng");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper ph?i trong tr?ng thái revise d? th?c hi?n g?i file");
            }


            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }

            string revisionDeadlineId = string.Empty;
            var researchConferencePhasesFound = paper.ResearchConferencePhase;
            if (researchConferencePhasesFound == null)
            {
                throw new NotFoundException("Không tìm th?y các giai do?n trong h?i ngh? nghiên c?u");
            }
            var researchConferenceDeadLine = researchConferencePhasesFound.RevisionRoundDeadlines;

            var validRevisionDeadline = researchConferenceDeadLine.FirstOrDefault(rcd => rcd.StartSubmissionDate <= dateNow && dateNow <= rcd.EndSubmissionDate);
            if (validRevisionDeadline == null)
            {
                throw new NotFoundException("Không tìm th?y các deadline h?p l?");
            }
            revisionDeadlineId = validRevisionDeadline.RevisionRoundDeadlineId;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                RevisionPaper? revisionPaper = null;

                if (paper.RevisionPaperId == null)
                {
                    revisionPaper = new RevisionPaper()
                    {
                        RevisionPaperId = Guid.NewGuid().ToString(),
                        RevisionRound = 1,
                        GlobalStatusId = pendingGlobalStatus.GlobalStatusId,
                        CreatedAt = await _timeProviderService.GetVietnamTime(),
                        ReviewAt = null,
                    };
                    paper.RevisionPaperId = revisionPaper.RevisionPaperId;
                    await _unitOfWork.RevisionPaperRepository.CreateRevisionPaperAsync(revisionPaper);
                    await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                }
                else
                {
                    revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
                    if (revisionPaper == null)
                    {
                        throw new BadRequestException($"Revision paper id {paper.RevisionPaperId} không tìm th?y trong h? th?ng");
                    }
                    //revisionPaper.RevisionRound = revisionPaper.RevisionRound + 1;
                    if (!string.IsNullOrEmpty(revisionDeadlineId))
                    {
                        var revisionPaperSubmissionFound = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByRevisionPaperIdAndDeadlineId(paper.RevisionPaperId, revisionDeadlineId);
                        if (revisionPaperSubmissionFound != null)
                        {
                            throw new BadRequestException($"B?n dã n?p revision, deadline di?n ra t? {revisionPaperSubmissionFound.RevisionDeadlineRound?.StartSubmissionDate} d?n {revisionPaperSubmissionFound.RevisionDeadlineRound?.EndSubmissionDate} này ");
                        }
                    }
                    revisionPaper.RevisionRound = revisionPaper.RevisionRound + 1;
                }
                var totalRevisionRoundAllowed = paper.Conference!.ResearchConferenceDetail!.RevisionAttemptAllowed;
                if (revisionPaper.RevisionRound > totalRevisionRoundAllowed)
                {
                    throw new BadRequestException($"Không th? n?p thêm paper submission vì dã quá {totalRevisionRoundAllowed} l?n, vui lòng ch? phán quy?t t? head reviewer!");
                }

                string? revisionFileUrl = null;
                if (request.RevisionPaperFile != null)
                {
                    if (request.RevisionPaperFile.ContentType == null)
                    {
                        throw new BadRequestException("Content type không h?p l?");
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
                    Title = request.Title,
                    Description = request.Description,

                };
                int result = 0;
                result += await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
                result += await _unitOfWork.RevisionPaperSubmissionRepository.CreateRevisionPaperSubmissionAsync(revisionPaperSubmissionObj);
                await _unitOfWork.CommitAsync();
                return result;
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
                throw new NotFoundException($"Không tìm th?y paper  id {request.PaperId} trong h? th?ng");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }
            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper submission id {request.RevisionPaperSubmissionId} trong h? th?ng");
            }
            var revisionPaperSubmissionDeadLine = revisionPaperSubmission.RevisionDeadlineRound;
            if (revisionPaperSubmissionDeadLine == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper deadline trong h? th?ng");
            }
            if (dateNow < revisionPaperSubmissionDeadLine.StartSubmissionDate || dateNow > revisionPaperSubmissionDeadLine.EndSubmissionDate)
            {
                throw new BadRequestException($"Deadline cho l?n tuong tác qua l?i n?m trong kho?ng {revisionPaperSubmissionDeadLine.StartSubmissionDate} d?n {revisionPaperSubmissionDeadLine.EndSubmissionDate} ");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Không tìm th?y user v?i id {userId} trong h? th?ng assign cho bài báo {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException($"Ch?c nang này ch? dành cho head reviewer.");

            }
            var feedBackList = new List<RevisionSubmissionFeedback>();
            foreach (var feedback in request.Feedbacks)
            {
                var feedbackObj = new RevisionSubmissionFeedback()
                {
                    RevisionSubmissionFeedbackId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Feedback = feedback.Feedback,
                    Response = null,
                    SortOrder = feedback.SortOrder,
                    CreatedAt = await _timeProviderService.GetVietnamTime(),
                    RevisionPaperSubmissionId = revisionPaperSubmission.RevisionPaperSubmissionId,
                };
                feedBackList.Add(feedbackObj);
            }
            return await _unitOfWork.RevisionSubmissionFeedbackRepository.CreateMultipleFeedbacksAsync(feedBackList);
        }

        public async Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request, string userId)
        {
            if (request.Responses == null || !request.Responses.Any())
            {
                throw new BadRequestException("Responses không du?c d? tr?ng.");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y paper  id {request.PaperId} trong h? th?ng");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }
            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper submission id {request.RevisionPaperSubmissionId} trong h? th?ng");
            }
            var revisionPaperSubmissionDeadLine = revisionPaperSubmission.RevisionDeadlineRound;
            if (revisionPaperSubmissionDeadLine == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper deadline trong h? th?ng");
            }
            if (dateNow < revisionPaperSubmissionDeadLine.StartSubmissionDate || dateNow > revisionPaperSubmissionDeadLine.EndSubmissionDate)
            {
                throw new BadRequestException($"Deadline cho l?n tuong tác qua l?i n?m trong kho?ng {revisionPaperSubmissionDeadLine.StartSubmissionDate} d?n {revisionPaperSubmissionDeadLine.EndSubmissionDate} ");
            }

            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);

            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }
            var feedBackList = new List<RevisionSubmissionFeedback>();
            foreach (var response in request.Responses)
            {
                var revisionSubmissionFeedback = await _unitOfWork.RevisionSubmissionFeedbackRepository.GetFeedbackByIdAsync(response.RevisionSubmissionFeedbackId);
                if (revisionSubmissionFeedback == null)
                {
                    throw new NotFoundException($"Không tìm th?y paper  id {response.RevisionSubmissionFeedbackId} trong h? th?ng");
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
                throw new BadRequestException($"Không th? chuy?n tr?ng thái pending");
            }
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var rejectGlobalStautus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());


            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            if (acceptedGlobalStatus == null || currentRevisePhase == null || pendingGlobalStatus == null || rejectGlobalStautus == null)
            {
                throw new NotFoundException("Không tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }

            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y paper  id {request.PaperId} trong h? th?ng");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }


            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không th? g?i review vì paper dang không trong tr?ng thái revise");
            }
            bool isReviewerValid = false;
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer != null)
            {
                isReviewerValid = true;
            }
            if (isReviewerValid == false)
            {
                var reviewerContract = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, paper.ConferenceId!);

                if (reviewerContract != null)
                {
                    isReviewerValid = true;
                }
            }
            if (isReviewerValid == false)
            {
                throw new BadRequestException($"B?n hi?n t?i không tìm th?y trong danh sách gán reviewer ho?c có b?t c? h?p d?ng nào v?i h?i ngh? v?i mã {paper.ConferenceId}");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper {request.RevisionPaperId} trong h? th?ng");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm th?y revision paper {request.RevisionPaperId} tuong ?ng v?i paper trong h? th?ng");
            }
            if (revisionPaper.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision này dang không trong tr?ng thái Pending nên không th? g?i revision review");
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
            string finalGlobalStatusId = string.Empty;
            if (request.GlobalStatus == GlobalStatusEnum.Accepted)
            {
                finalGlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
            }
            else
            {
                finalGlobalStatusId = rejectGlobalStautus.GlobalStatusId;
            }
            var revisionPaperReviewObj = new RevisionPaperReview()
            {
                RevisionPaperReviewId = Guid.NewGuid().ToString(),
                GlobalStatusId = finalGlobalStatusId,
                Note = request.Note,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
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
                throw new BadRequestException("Không th? chuy?n tr?ng thái pending cho giai do?n revise");
            }
            if (currentRevisePhase == null || pendingGlobalStatus == null || acceptedGlobalStatus == null || cameraReadyPaperPhase == null || rejectGlobalStautus == null)
            {
                throw new NotFoundException("Không tìm th?y các tr?ng thái trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y  paper {request.PaperId} trong h? th?ng");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper dang không ? trong tr?ng thái revise");
            }
            //dùng hàm get
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm th?y  revision paper {request.RevisionPaperId} trong h? th?ng");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Paper {request.PaperId} không thu?c revision paper {request.RevisionPaperId}");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"B?n không có quy?n h?n d? quy?t d?nh bài báo này");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new BadRequestException($"B?n không ph?i là head reviewer d? quy?t d?nh status c?a bài báo này");
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = 0;
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        //update instance get:
                        revisionPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        revisionPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        paper.PaperPhaseId = cameraReadyPaperPhase.PaperPhaseId;
                        break;

                    case GlobalStatusEnum.Rejected:
                        revisionPaper.GlobalStatusId = rejectGlobalStautus.GlobalStatusId;
                        revisionPaper.ReviewAt = await _timeProviderService.GetVietnamTime();
                        var rootAuthor = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
                        var ticket = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(rootAuthor!.UserId, paper.ConferenceId!);
                        await _ticketService.RefundAuthorCloneFunction(rootAuthor!.UserId, ticket.TicketId, "Revise paper c?a b?n dã b? t? ch?i");
                        break;

                    default:
                        throw new BadRequestException("Tr?ng thái không kh? d?ng");
                }
                //call hàm update
                result += await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);

                await _unitOfWork.CommitAsync();
                return result;
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
                throw new NotFoundException($"Không tìm th?y  paper {request.PaperId} trong h? th?ng");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"B?n không có quy?n h?n d? truy c?p bài báo này");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm th?y revision paper id {request.RevisionPaperId} trong paper {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException($"B?n không ph?i là head reviewer d? xem danh sách này");
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
                throw new BadRequestException($"Bài báo v?i id {request.PaperId} không t?n t?i.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.CameraReadyStartDate || dateNow > activeCurrentPhase.CameraReadyEndDate)
            {
                throw new BadRequestException($"Giai do?n n?p camera ready  di?n ra t? {activeCurrentPhase.CameraReadyStartDate} d?n {activeCurrentPhase.CameraReadyEndDate}");
            }

            // Check if paper already has a camera ready
            if (!string.IsNullOrEmpty(paper.CameraReadyId))
            {
                throw new BadRequestException($"bài báo v?i mã {request.PaperId} dã có camera ready n?p s?n r?i.");
            }

            // Validate that the user is the presenter of the paper
            //if (paper.PresenterId != userId)
            //{
            //    throw new BadRequestException("You are not authorized to create camera ready for this paper.");
            //}

            // Validation: Paper must have either:
            // 1. RevisionPaper with GlobalStatus = "Accepted", OR
            // 2. FullPaper with ReviewStatus = "Accepted"

            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }


            bool isValidPaper = false;


            if (!string.IsNullOrEmpty(paper.FullPaperId))
            {
                // Check if FullPaper exists and has ReviewStatus = "Accepted"
                var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId);
                if (fullPaper != null && fullPaper.ReviewStatus.ReviewStatusId != null)
                {
                    var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
                    if (fullPaper.ReviewStatusId == acceptedReviewStatus.ReviewStatusId)
                    {
                        isValidPaper = true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(paper.RevisionPaperId))
            {
                // Check if RevisionPaper exists and has GlobalStatus = "Accepted"
                var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
                if (revisionPaper != null && revisionPaper.GlobalStatusId != null)
                {
                    var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    if (revisionPaper.GlobalStatusId == acceptedGlobalStatus.GlobalStatusId)
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
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                Description = request.Description,
                ReviewAt = null,
                Title = request.Title,
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

            //// Validate that the user is a head reviewer of the paper
            //var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
            //if (paperReviewer == null)
            //{
            //    throw new BadRequestException("You are not a reviewer of this paper.");
            //}

            //if (paperReviewer.IsHeadReviewer != true)
            //{
            //    throw new BadRequestException("Only head reviewers can update camera ready.");
            //}
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paper.PaperId);
            if (paperAuthors == null)
            {
                throw new NotFoundException("Không tìm th?y b?t c? paper author nào");
            }
            var paperOwnerShip = paperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (paperOwnerShip == null)
            {
                throw new BadRequestException("B?n không s? h?u bài báo này");

            }



            cameraReady.Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : cameraReady.Title;
            cameraReady.Description = !string.IsNullOrWhiteSpace(request.Title) ? request.Description : cameraReady.Description;


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
                throw new BadRequestException($"user v?i ID {userId} không t?n t?i.");
            }
            if (request.reviewStatus == ReviewStatusEnum.Pending)
            {
                throw new BadRequestException("Không th? thành pending cho. Ch? có th? accept ho?c reject");
            }

            //// Check if user is a reviewer (either Local Reviewer or External Reviewer)
            //var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Local Reviewer");
            //var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("External Reviewer");

            //if (localReviewerRole == null || externalReviewerRole == null)
            //{
            //    throw new BadRequestException("Reviewer roles do not exist in the system.");
            //}

            //var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);
            //var hasReviewerRole = userRoles.Any(ur => ur.RoleId == localReviewerRole.RoleId || ur.RoleId == externalReviewerRole.RoleId);

            //if (!hasReviewerRole)
            //{
            //    throw new BadRequestException("User must have Local Reviewer or External Reviewer role to submit a review.");
            //}

            // Validate that the full paper exists
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper v?i id {request.FullPaperId} không t?n t?i.");
            }

            // Validate that the user is assigned as a reviewer to this paper
            var paper = await _unitOfWork.PaperRepository.GetPaperByFullPaperIdAsync(request.FullPaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Bài báo v?i full paper ID {request.FullPaperId} không t?n t?i.");
            }

            bool isReviewerValid = false;
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
            if (paperReviewer != null)
            {
                isReviewerValid = true;
            }
            if (isReviewerValid == false)
            {
                var reviewerContract = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, paper.ConferenceId!);

                if (reviewerContract != null)
                {
                    isReviewerValid = true;
                }
            }
            if (isReviewerValid == false)
            {
                throw new BadRequestException($"B?n hi?n t?i không tìm th?y trong danh sách gán reviewer ho?c có b?t c? h?p d?ng nào v?i h?i ngh? v?i mã {paper.ConferenceId}");
            }





            // Check if the user has already submitted a review for this full paper
            //var existingReview = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(request.FullPaperId, userId);
            //if (existingReview != null)
            //{
            //    throw new BadRequestException("You have already submitted a review for this full paper.");
            //}

            // Validate that the full paper is in "Pending" review status
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            if (pendingReviewStatus == null)
            {
                throw new BadRequestException("Tr?n thái pending không t?n t?i trong h? th?ng");
            }

            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException("Full paper ph?i trong tr?ng thái pending d? g?i fullpaper review.");
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
            var decideStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(request.reviewStatus.GetDescription());

            // Create the full paper review
            var fullPaperReview = new FullPaperReview
            {
                FullPaperReviewId = Guid.NewGuid().ToString(),
                FullPaperId = request.FullPaperId,
                ReviewerId = userId,
                ReviewStatusId = decideStatus.ReviewStatusId,
                Note = request.Note,
                FeedbackToAuthor = request.FeedbackToAuthor,
                FeedbackMaterialUrl = feedbackMaterialUrl,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
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
                throw new BadRequestException($"Camera ready v?i ID {request.CameraReadyId} không t?n t?i.");
            }

            // Validate that the camera ready is in "Pending" status
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingGlobalStatus == null)
            {
                throw new BadRequestException("Giai do?n pending không t?n t?i trong h? th?ng");
            }

            if (cameraReady.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Camera ready ph?i trong tr?ng thái pending d? c?p nh?t status");
            }

            // Validate that the user is a head reviewer for the paper associated with this camera ready
            var paper = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(request.CameraReadyId);
            if (paper == null)
            {
                throw new BadRequestException($"bài báo v?i camera id {request.CameraReadyId} không t?n t?i ho?c không liên k?t v?i nhau.");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paper.PaperId);

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
            if (paperReviewer == null)
            {
                throw new BadRequestException("B?n không có quy?n trong bài báo này");
            }
            if (paperReviewer.IsHeadReviewer != true)
            {
                throw new BadRequestException("Ch? head reviewer m?i có th? quy?t d?nh bài báo");
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
                    var rootAuthor = basePaper!.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
                    var ticket = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(rootAuthor!.UserId, basePaper.ConferenceId!);
                    await _ticketService.RefundAuthorCloneFunction(rootAuthor!.UserId, ticket.TicketId, "Camera ready paper c?a b?n dã b? t? ch?i");
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
            cameraReady.ReviewAt = await _timeProviderService.GetVietnamTime();
            return await _unitOfWork.CameraReadyRepository.UpdateCameraReadyAsync(cameraReady);
        }


        public async Task<List<PapersAssignedToReviewerResponse>> GetAllAssignedPapersToAReviewer(string userId, string conferenceId)
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

        public async Task<List<Paper>> GetSubmittedPaper(string userId, string? confId)
        {
            // Use the new repository method to get papers by user ID in a single query
            var submittedPapers = await _unitOfWork.PaperAuthorRepository.GetPapersByUserIdAsync(userId);
            if (confId != null) submittedPapers.Where(p => p.ConferenceId == confId).ToList();

            return submittedPapers;
        }

        public async Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId)
        {
            // Step 1: Fetch the main Paper entity. This is our starting point.
            // We get Phase and CameraReady here because they are included in the repo method.
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdWithPhaseAsync(paperId);

            if (paper == null)
            {
                throw new KeyNotFoundException($"Không tìm th?y paper v?i id {paperId}");
            }

            var researchConferencePhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByPaperId(paper.PaperId);
            if (researchConferencePhase == null) throw new BadRequestException("Paper này chua thu?c v? researchPhase nào");
            var roundDeadline = await _unitOfWork.ResearchConferencePhaseRepository.GetRevisionRoundDeadlinesByPhaseIdAsync(researchConferencePhase.ResearchConferencePhaseId);

            //get all authors
            var allAuthor = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
            //get rootauthor
            var paperRootAuthor = allAuthor.FirstOrDefault(pa => pa.IsRootAuthor == true);
            var RootAuthor = await _unitOfWork.UserRepository.GetUserByUserId(paperRootAuthor.UserId);
            var coAuthorIds = allAuthor.Where(pa => pa.UserId != RootAuthor.UserId).Select(paper => paper.UserId).ToList();
            List<User> coAuthors = new List<User>();
            if (coAuthorIds.Count() > 0)
            {
                foreach (var authorId in coAuthorIds)
                {
                    User CoAuthor = await _unitOfWork.UserRepository.GetUserByUserId(authorId);
                    if (CoAuthor != null)
                    {
                        coAuthors.Add(CoAuthor);
                    }
                }
            }



            var abstractEntity = paper.AbstractId != null
            ? await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId)
            : null;

            var fullPaperEntity = paper.FullPaperId != null
                ? await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId)
                : null;

            var revisionPaperEntity = paper.RevisionPaperId != null
                ? await _unitOfWork.RevisionPaperRepository.GetDetailRevisionPaper(paper.RevisionPaperId)
                : null;

            // Step 5: Map all the fetched entities into our clean DTO response model.
            var response = new PaperDetailResponseDtoDetail
            {
                PaperId = paper.PaperId,
                Title = paper.Title,
                Description = paper.Description,
                Created = paper.CreatedAt,
                RootAuthor = RootAuthor != null ? new Author { UserId = RootAuthor.UserId, FullName = RootAuthor.FullName } : null,
                CoAuthors = coAuthors?.Select(user => new Author
                {
                    UserId = user.UserId,
                    FullName = user.FullName
                }).ToList(),
                //ResearchPhase = researchConferencePhase != null ? new ResearchPhaseDtoDetail
                //{
                //    ResearchConferencePhaseId = researchConferencePhase.ResearchConferencePhaseId,
                //    RegistrationStartDate = researchConferencePhase.RegistrationStartDate,
                //    RegistrationEndDate = researchConferencePhase.RegistrationEndDate,
                //    FullPaperStartDate = researchConferencePhase.FullPaperStartDate,
                //    FullPaperEndDate = researchConferencePhase.FullPaperEndDate,
                //    ReviewStartDate = researchConferencePhase.ReviewStartDate,
                //    ReviewEndDate = researchConferencePhase.ReviewEndDate,
                //    ReviseStartDate = researchConferencePhase.ReviseStartDate,
                //    ReviseEndDate = researchConferencePhase.ReviseEndDate,
                //    CameraReadyStartDate = researchConferencePhase.CameraReadyStartDate,
                //    CameraReadyEndDate = researchConferencePhase.ReviewEndDate,
                //    ConferenceId = researchConferencePhase.ConferenceId
                //} : null,
                ResearchPhase = paper.ResearchConferencePhase != null ? new ResearchPhaseDtoDetail
                {
                    ResearchConferencePhaseId = paper.ResearchConferencePhase.ResearchConferencePhaseId,
                    RegistrationStartDate = paper.ResearchConferencePhase.RegistrationStartDate,
                    RegistrationEndDate = paper.ResearchConferencePhase.RegistrationEndDate,
                    FullPaperStartDate = paper.ResearchConferencePhase.FullPaperStartDate,
                    FullPaperEndDate = paper.ResearchConferencePhase.FullPaperEndDate,
                    ReviewStartDate = paper.ResearchConferencePhase.ReviewStartDate,
                    ReviewEndDate = paper.ResearchConferencePhase.ReviewEndDate,
                    ReviseStartDate = paper.ResearchConferencePhase.ReviseStartDate,
                    ReviseEndDate = paper.ResearchConferencePhase.ReviseEndDate,
                    CameraReadyStartDate = paper.ResearchConferencePhase.CameraReadyStartDate,
                    CameraReadyEndDate = paper.ResearchConferencePhase.CameraReadyEndDate,
                    ConferenceId = paper.ConferenceId
                } : null,

                // Map properties we already have from the initial query
                CurrentPhase = paper.PaperPhase != null ? new PaperPhaseDtoDetail
                {
                    PaperPhaseId = paper.PaperPhase.PaperPhaseId,
                    PhaseName = paper.PaperPhase.PhaseName
                } : null,

                CameraReady = paper.CameraReady != null ? new CameraReadyDtoDetail
                {
                    CameraReadyId = paper.CameraReady.CameraReadyId,
                    FileUrl = paper.CameraReady.CameraReadyUrl,
                    Status = paper.CameraReady.GlobalStatus?.Name, // Safe navigation
                    Title = paper.CameraReady.Title,
                    Description = paper.CameraReady.Description,
                    Created = paper.CameraReady.CreatedAt,
                    Updated = paper.CameraReady.ReviewAt
                } : null,

                // Map the result from the parallel tasks
                Abstract = abstractEntity != null ? new AbstractDtoDetail
                {
                    AbstractId = abstractEntity.AbstractId,
                    FileUrl = abstractEntity.AbstractUrl,
                    Status = abstractEntity.GlobalStatus?.Name,
                    Title = abstractEntity.Title,
                    Description = abstractEntity.Description,
                    Created = abstractEntity.CreatedAt,
                    Updated = abstractEntity.ReviewAt
                } : null,

                FullPaper = fullPaperEntity != null ? new FullPaperDtoDetail
                {
                    FullPaperId = fullPaperEntity.FullPaperId,
                    FileUrl = fullPaperEntity.FullPaperUrl,
                    ReviewStatus = fullPaperEntity.ReviewStatus?.Name,
                    Title = fullPaperEntity.Title,
                    Description = fullPaperEntity.Description,
                    Created = fullPaperEntity.CreatedAt,
                    Updated = fullPaperEntity.ReviewAt
                } : null,
                revisionDeadline = roundDeadline?.Select(r => new RevisionDeadlineDetail
                {
                    RevisionRoundDeadlineId = r?.RevisionRoundDeadlineId,
                    RoundNumber = r?.RoundNumber,
                    StartSubmissionDate = r?.StartSubmissionDate,
                    EndSubmissionDate = r?.EndSubmissionDate,
                    ResearchConferencePhaseId = researchConferencePhase.ResearchConferencePhaseId
                }).ToList(),

                // Use a helper method for complex mapping to keep this clean
                RevisionPaper = revisionPaperEntity != null
                    ? MapRevisionToDto(revisionPaperEntity, researchConferencePhase, roundDeadline)
                    : null
            };

            return response;
        }

        private RevisionPaperDtoDetail MapRevisionToDto(ConfRadar.Repositories.Models.RevisionPaper entity, ResearchConferencePhase phase, List<RevisionRoundDeadline> deadlines)
        {
            if (entity == null) return null;

            return new RevisionPaperDtoDetail
            {
                RevisionPaperId = entity.RevisionPaperId,
                RevisionRound = entity.RevisionRound,
                Created = entity.CreatedAt,
                Updated = entity.ReviewAt,
                OverallStatus = entity.GlobalStatus?.Name,
                Reviews = entity.RevisionPaperReviews?.Select(review => new RevisionReviewDtoDetail
                {
                    ReviewId = review.RevisionPaperReviewId,
                    Note = review.Note,
                    FeedBackToAuthor = review.FeedbackToAuthor,
                    FeedbackMaterialURL = review.FeedbackMaterialUrl,
                    ReviewedAt = review.CreatedAt ?? default // Use default if nullable
                }).ToList() ?? new List<RevisionReviewDtoDetail>(),

                Submissions = entity.RevisionPaperSubmissions?.Select(sub => new RevisionSubmissionDtoDetail
                {
                    SubmissionId = sub.RevisionPaperSubmissionId,
                    FileUrl = sub.RevisionPaperUrl,
                    Title = sub.Title,
                    Description = sub.Description,
                    RevisionRoundId = sub.RevisionDeadlineRoundId,
                    Feedbacks = sub.RevisionSubmissionFeedbacks?.Select(fb => new FeedbackDtoDetail
                    {
                        FeedbackId = fb.RevisionSubmissionFeedbackId,
                        FeedBack = fb.Feedback,
                        Response = fb.Response,
                        Order = fb.SortOrder ?? 0,
                        CreatedAt = fb.CreatedAt ?? default
                    }).ToList() ?? new List<FeedbackDtoDetail>()
                }).ToList() ?? new List<RevisionSubmissionDtoDetail>()
            };
        }


        public async Task<List<PendingAbstractResponse>> GetListPendingAbstract(string? confId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm th?y tr?ng thái trong h? th?ng");
            }
            var listAbstract = await _unitOfWork.AbstractRepository.GetAllPendingAbstractsAsync(pendingGlobalStatus.GlobalStatusId);
            if (!string.IsNullOrEmpty(confId)) listAbstract = listAbstract.Where(abs => abs.ConferenceId == confId).ToList();
            return listAbstract;
        }

        public async Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases()
        {
            return await _unitOfWork.PaperPhaseRepository.GetAllPaperPhasesAsync();
        }


        public async Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersByReviewerId(string userId, string? confId)
        {
            var allAssignedPapers = await _unitOfWork.PaperReviewerRepository.getAllAssignedPapers(userId);


            if (!string.IsNullOrEmpty(confId))
            {
                allAssignedPapers = allAssignedPapers.Where(p => p.ConferenceId == confId).ToList();
            }

            var groupedByConference = allAssignedPapers.GroupBy(p => p.ConferenceId).ToList();
            List<ConferenceWithAssignedPapersResponse> response = new();
            foreach (var conferenceGroup in groupedByConference)
            {
                var firstPaperInGroup = conferenceGroup.First();
                if (firstPaperInGroup.ConferenceId == null) continue;
                var conferenceResponse = new ConferenceWithAssignedPapersResponse()
                {
                    ConferenceId = conferenceGroup.Key,
                    ConferenceName = firstPaperInGroup.Conference.ConferenceName,
                    AssignedPapers = new List<BasicAssignedPaperResponse>()
                };
                foreach (var paper in conferenceGroup)
                {
                    var paperResponse = new BasicAssignedPaperResponse
                    {
                        PaperId = paper.PaperId,
                        Title = paper.Title,

                        Description = paper.Description,
                        CreatedAt = paper.CreatedAt,
                        PaperPhaseId = paper.PaperPhaseId,

                        PaperPhaseName = paper.PaperPhase?.PhaseName,


                        AbstractId = paper.AbstractId,
                        FullPaperId = paper.FullPaperId,
                        CameraReadyId = paper.CameraReadyId,
                        RevisionPaperId = paper.RevisionPaperId
                    };
                    conferenceResponse.AssignedPapers.Add(paperResponse);
                }

                response.Add(conferenceResponse);
            }
            return response;
        }

        //public async Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersGroupedByConference(string userId, string? confId)
        //{

        //    // Get assigned papers using the existing method logic
        //    var assignedPapers = await GetAssignedPapersByReviewerId(userId, confId);
        //    if (!assignedPapers.Any())
        //    {
        //        return new List<ConferenceWithAssignedPapersResponse>();
        //    }

        //    // Get paper reviewer info to include reviewer details
        //    var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAsync(userId);

        //    // Group papers by conference
        //    var groupedPapers = assignedPapers.GroupBy(p => p.ConferenceId).ToList();
        //    var response = new List<ConferenceWithAssignedPapersResponse>();

        //    foreach (var conferenceGroup in groupedPapers)
        //    {
        //        var firstPaper = conferenceGroup.First();
        //        var conference = firstPaper.Conference;

        //        if (conference == null) continue;

        //        var assignedPapersForConference = new List<BasicAssignedPaperResponse>();

        //        foreach (var paper in conferenceGroup)
        //        {
        //            var reviewerInfo = paperReviewers.FirstOrDefault(pr => pr.PaperId == paper.PaperId);

        //            assignedPapersForConference.Add(new BasicAssignedPaperResponse
        //            {
        //                PaperId = paper.PaperId,
        //                Title = paper.Title,
        //                Description = paper.Description,
        //                CreatedAt = paper.CreatedAt,
        //                PaperPhaseId = paper.PaperPhaseId,
        //                PaperPhaseName = paper.PaperPhase?.PaperPhaseName,

        //                // Basic IDs only - no full objects as requested
        //                AbstractId = paper.AbstractId,
        //                FullPaperId = paper.FullPaperId,
        //                CameraReadyId = paper.CameraReadyId,
        //                RevisionPaperId = paper.RevisionPaperId,

        //                // Reviewer info
        //                IsHeadReviewer = reviewerInfo?.IsHeadReviewer ?? false,
        //                AssignedAt = reviewerInfo?.CreatedAt
        //            });
        //        }

        //        response.Add(new ConferenceWithAssignedPapersResponse
        //        {
        //            ConferenceId = conference.ConferenceId,
        //            ConferenceName = conference.ConferenceName,
        //            AssignedPapers = assignedPapersForConference.OrderBy(ap => ap.CreatedAt).ToList()
        //        });
        //    }

        //    // Sort by conference name for consistent output
        //    return response.OrderBy(r => r.ConferenceName).ToList();

        //    //foreach (string p in paperIds)
        //    //{
        //    //    if (p != null) AssignedPapers.Add(await _unitOfWork.PaperRepository.GetPaperByIdAsync(p));
        //    //}
        //    var AssignedPapers = await _unitOfWork.PaperReviewerRepository.getAllAssignedPapers(userId);

        //    return AssignedPapers;
        //}

        public async Task<List<CameraReadyDtoDetail>> ListPendingCameraReady()
        {
            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            List<CameraReady> pendingCameraready = await _unitOfWork.CameraReadyRepository.GetCameraBystatusName(pendingStatus!.Name!);
            List<CameraReadyDtoDetail> result = new List<CameraReadyDtoDetail>();
            foreach (CameraReady c in pendingCameraready)
            {
                var paperId = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(c.CameraReadyId);
                CameraReadyDtoDetail responseDTO = new CameraReadyDtoDetail
                {
                    CameraReadyId = c?.CameraReadyId,
                    FileUrl = c?.CameraReadyUrl,
                    Status = c?.GlobalStatus.Name,
                    RootPaperId = paperId.PaperId
                };
                result.Add(responseDTO);
            }
            return result;
        }

        public async Task<List<FullPaperDtoDetail>> ListPendingfullpaper()
        {
            var pendingStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            List<FullPaper> pendingFullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByStatusName(pendingStatus!.Name!);
            List<FullPaperDtoDetail> result = new List<FullPaperDtoDetail>();
            foreach (FullPaper fp in pendingFullPaper)
            {
                var paper = await _unitOfWork.PaperRepository.GetPaperByFullPaperIdAsync(fp.FullPaperId);
                FullPaperDtoDetail fullPaperDto = new FullPaperDtoDetail
                {
                    FullPaperId = fp?.FullPaperId,
                    FileUrl = fp?.FullPaperUrl,
                    ReviewStatus = fp?.ReviewStatus.Name,
                    RootPaperId = paper.PaperId
                };
                result.Add(fullPaperDto);
            }
            return result;
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

        public async Task<List<UnAssignAbstractResponse>> GetUnassignAbstractList()
        {
            var unassignAbstract = await _unitOfWork.PaperRepository.GetUnAssignAbstract();
            return unassignAbstract;

        }

        public async Task<PaperDetailForReviewerResponse?> GetPaperDetailForReviewer(string paperId, string userId)
        {
            var paperReviewerCheck = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId);
            if (paperReviewerCheck == null)
            {
                throw new BadRequestException("B?n không có quy?n h?n d? xem paper này");

            }
            var result = await _unitOfWork.PaperRepository.GetPaperDetailForReviewer(paperId, userId);
            return result;
        }

        public Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId)
        {
            return _unitOfWork.PaperWaitListRepository.GetCustomerWaitList(userId);
        }

        public async Task<LeaveWaitListResponse> LeaveWaitList(string userId, string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new BadRequestException($"H?i ngh? v?i id {conferenceId} không t?n t?i trong h? th?ng");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound == null)
            {
                throw new BadRequestException($"Không t?n hàng d?i d? xóa");
            }
            var result = await _unitOfWork.PaperWaitListRepository.DeletePaperWaitListAsync(waitListFound);

            return new LeaveWaitListResponse()
            {
                ConferenceId = conferenceId,
                IsLeaved = result
            };
        }

        public async Task<AddWaitListResponse> AddWaitList(string userId, string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new BadRequestException($"H?i ngh? v?i id {conferenceId} không t?n t?i trong h? th?ng");
            }
            var conferencePhases = conference.ResearchConferencePhases;
            var firstPhase = conferencePhases.FirstOrDefault(cp => cp.IsActive == true && cp.IsWaitlist == false);
            var waitListPhase = conferencePhases.FirstOrDefault(cp => cp.IsActive == true && cp.IsWaitlist == true);
            if (firstPhase != null && waitListPhase != null)
            {
                throw new BadRequestException("Hi?n t?i h?i ngh? dang ? trong 2 giai do?n b? trùng nhau. Xin vui lòng liên h? ban t? ch?c");
            }
            if (firstPhase == null)
            {
                throw new BadRequestException("B?n ch? có th? vô hàng d?i trong khi ? giai do?n d?u");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound != null)
            {
                throw new BadRequestException($"B?n dã ? trong hàng d?i r?i");
            }
            var paperWaitListNotifiedStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());
            if (paperWaitListNotifiedStatus == null)
            {
                throw new NotFoundException("Không tìm th?y tr?ng thái hàng d?i trong h? th?ng");
            }
            var waitListObj = new PaperWaitList()
            {
                PaperWaitListId = Guid.NewGuid().ToString(),
                ConferenceId = conferenceId,
                UserId = userId,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                NotifiedAt = null,
                WaitListStatusId = paperWaitListNotifiedStatus.WaitListStatusId,
            };
            var result = await _unitOfWork.PaperWaitListRepository.CreatePaperWaitListAsync(waitListObj);
            return new AddWaitListResponse()
            {
                ConferenceId = conferenceId,
                IsAdded = result > 0 ? true : false
            };

        }

        public async Task<int> UpdateAbstract(UpdateAbstractRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var paperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (paperPhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException($"Không tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y bài báo v?i mã {request.PaperId} trong h? th?ng");
            }
            if (paper.AbstractId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chua có abstract d? ch?nh s?a");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId);
            if (abstractPaper!.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Abstract hi?n không ? tr?ng thái 'Pending', nên không th? ch?nh s?a.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y các giai do?n cho h?i ngh? nghiên c?u {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Giai do?n s?a abstract di?n ra t? {activeCurrentPhase.RegistrationStartDate} d?n {activeCurrentPhase.RegistrationEndDate} nên b?n không th? ch?nh s?a");
            }

            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hi?n t?i không dang trong quá trình s?a abstract");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }
            var submitterReviewContracts = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            List<PaperAuthor> paperAuthorList = new List<PaperAuthor>();
            if (request.CoAuthorId != null && request.CoAuthorId.Count > 0 && submitterReviewContracts.Count() > 0)
            {
                foreach (var coauthorId in request.CoAuthorId)
                {
                    if (coauthorId == userId)
                    {
                        throw new BadRequestException("B?n không th? thêm chính mình làm co-author.");
                    }
                    //check coauthor có là reviewer cho bài báo này
                    bool isCoauthorReviewerInPaperReviewer = submitterReviewContracts.Any(pr => pr.UserId == coauthorId);
                    if (isCoauthorReviewerInPaperReviewer == true)
                    {
                        throw new BadRequestException($"Ngu?i dùng {coauthorId} dang là reviewer c?a bài báo này, không th? thêm làm co-author.");
                    }

                    //check coauthor có là external reviewer có contract vs h?i ngh? 
                    var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(coauthorId, paper.Conference!.ConferenceId);
                    if (reviewerContractFound != null)
                    {
                        if (reviewerContractFound.IsActive == true)
                        {
                            throw new BadRequestException($"Co author v?i id {coauthorId} hi?n dang có h?p d?ng reviewer");
                        }
                    }
                    var paperAuthorObj = new PaperAuthor()
                    {
                        IsPresenter = false,
                        UserId = coauthorId,
                        PaperId = request.PaperId,
                        IsRootAuthor = false,
                    };
                    paperAuthorList.Add(paperAuthorObj);

                }
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
                abstractPaper.AbstractUrl = abstractFileUrl;
            }
            var oldPaperCoAuthors = paper.PaperAuthors.Where(pa => pa.IsRootAuthor == false).ToList();
            abstractPaper.Title = string.IsNullOrWhiteSpace(request.Title) ? abstractPaper.Title : request.Title;
            abstractPaper.Description = string.IsNullOrWhiteSpace(request.Description) ? abstractPaper.Description : request.Description;






            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int finalResult = 0;
                finalResult += await _unitOfWork.AbstractRepository.UpdateAbstractAsync(abstractPaper);
                if (oldPaperCoAuthors.Count() > 0 && request.CoAuthorId != null && request.CoAuthorId.Count() > 0)
                {
                    finalResult += await _unitOfWork.PaperAuthorRepository.DeleteMutiplePaperAuthorAsync(oldPaperCoAuthors);
                }
                if (paperAuthorList.Count > 0)
                {
                    finalResult += await _unitOfWork.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(paperAuthorList);
                }
                await _unitOfWork.CommitAsync();
                return finalResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId)
        {
            var pendingFullPaperReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription());

            if (fullPaperPhase == null || pendingFullPaperReviewStatus == null)
            {
                throw new NotFoundException($"Không tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y bài báo v?i mã {request.PaperId} trong h? th?ng");
            }
            if (paper.FullPaperId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chua có fullpaper d? ch?nh s?a");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId);
            if (fullPaper!.ReviewStatusId != pendingFullPaperReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper hi?n không ? tr?ng thái 'Pending', nên không th? ch?nh s?a. Tr?ng thái hi?n t?i là {fullPaper.ReviewStatus?.Name}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y các giai do?n cho h?i ngh? nghiên c?u {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai do?n s?a full paper di?n ra t? {activeCurrentPhase.FullPaperStartDate} d?n {activeCurrentPhase.FullPaperEndDate} nên b?n không th? ch?nh s?a");
            }

            if (paper.PaperPhaseId != fullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hi?n t?i không dang trong quá trình s?a full paper");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
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
            fullPaper.Title = string.IsNullOrWhiteSpace(request.Title) ? fullPaper.Title : request.Title;
            fullPaper.Description = string.IsNullOrWhiteSpace(request.Description) ? fullPaper.Description : request.Description;
            return await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);

        }

        public async Task<int> UpdateRevisionPaperSubmission(UpdateRevisionPaperRevisionSubmissionRequest request, string userId)
        {
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (currentRevisePhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException($"Không th? tìm th?y tr?ng thái tuong ?ng trong h? th?ng");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm th?y trong h? th?ng");
            }
            if (paper.RevisionPaperId == null)
            {
                throw new NotFoundException($"Mã bài báo {request.PaperId} không tìm th?y revision id trong h? th?ng");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm th?y  giai do?n nào dang di?n ra cho h?i ngh? nghiên c?u {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai do?n revise di?n ra t? {activeCurrentPhase.ReviseStartDate} d?n {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper ph?i trong tr?ng thái revise d? th?c hi?n update");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"B?n không có quy?n s? h?u bài báo này");
            }
            var revisionPaperFound = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
            if (revisionPaperFound == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper v?i id {paper.RevisionPaperId}");
            }
            if (revisionPaperFound.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision paper ph?i trong tr?ng thái pending d? th?c hi?n update");
            }
            var revisionPaperSubmissionsList = revisionPaperFound.RevisionPaperSubmissions;
            if (revisionPaperSubmissionsList == null || !revisionPaperSubmissionsList.Any())
            {
                throw new NotFoundException("Không tìm th?y danh sách revision paper submission");
            }
            var currentRevisionPaperSubmission = revisionPaperSubmissionsList.FirstOrDefault(rps => rps.RevisionPaperSubmissionId == request.RevisionPaperSubmissionId);
            if (currentRevisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm th?y revision paper submission v?i id {request.RevisionPaperSubmissionId}");
            }
            var currentRevisionPaperSubmissionDeadline = currentRevisionPaperSubmission.RevisionDeadlineRound;
            if (currentRevisionPaperSubmissionDeadline == null)
            {
                throw new NotFoundException("Không tìm th?y thông tin deadline c?a revision submission này");
            }
            if (dateNow < currentRevisionPaperSubmissionDeadline!.StartSubmissionDate || dateNow > currentRevisionPaperSubmissionDeadline!.EndSubmissionDate)
            {
                throw new BadRequestException($"B?n không th? ch?nh s?a vì deadline revision submission này t? {currentRevisionPaperSubmissionDeadline.StartSubmissionDate} d?n {currentRevisionPaperSubmissionDeadline.EndSubmissionDate}");
            }
            var revisionSubmissionFeedbackList = currentRevisionPaperSubmission.RevisionSubmissionFeedbacks;
            if (revisionSubmissionFeedbackList.Any())
            {
                throw new BadRequestException($"B?n không th? ch?nh s?a vì  revision submission này vì hi?n t?i dã có head reviewer dua ra dánh giá. ");
            }
            var result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {

                currentRevisionPaperSubmission.Title = string.IsNullOrWhiteSpace(request.Title) ? currentRevisionPaperSubmission.Title : request.Title;
                currentRevisionPaperSubmission.Description = string.IsNullOrWhiteSpace(request.Description) ? currentRevisionPaperSubmission.Description : request.Description;
                string? revisionFileUrl = null;
                if (request.RevisionPaperFile != null)
                {
                    if (request.RevisionPaperFile.ContentType == null)
                    {
                        throw new BadRequestException("Content type không h?p l?");
                    }
                    using var stream = request.RevisionPaperFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.RevisionPaperFile.FileName);
                    var baseUri = _objectStorageSettings.Value.EndPoint;
                    var objectStorageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.revisionpaperfile.ToString(), uniqueFileName, stream, request.RevisionPaperFile.ContentType);
                    revisionFileUrl = baseUri + objectStorageFileUrl;
                    currentRevisionPaperSubmission.RevisionPaperUrl = revisionFileUrl;
                }
                result = await _unitOfWork.RevisionPaperSubmissionRepository.UpdateRevisionPaperSubmissionAsync(currentRevisionPaperSubmission);
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
