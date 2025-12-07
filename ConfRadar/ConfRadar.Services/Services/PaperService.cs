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
using ConfRadar.Shared.DTO.User;
using ConfRadar.Shared.DTO.WaitList;
using Microsoft.Extensions.Options;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        #region nộp paper

        Task<int> SubmitAbstract(CreateAbstractRequest request, string userId); //
        Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId); //
        Task<string> SubmitReviewForFullPaper(CreateFullPaperReviewRequest request, string userId); //

        Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId);//
        //feed back + response dùng chung revise start-end
        Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId);//
        Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request, string userId);//
        //dùng revise review start -end
        Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId);//
        Task<string> CreateCameraReady(CreateCameraReadyRequest request, string userId);//

        #endregion



        #region update paper
        Task<int> UpdateAbstract(UpdateAbstractRequest request, string userId); //
        Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId);//
        Task<int> UpdateRevisionPaperSubmission(UpdateRevisionPaperRevisionSubmissionRequest request, string userId);//
        Task<int> UpdateCameraReady(UpdateCameraReadyRequest request, string userId);//
        Task<int> UpdatePaper(UpdatePaperRequest request, string userId); //
        #endregion


        #region quyết định
        Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId); //
        Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId);//
        Task<int> DecideReviseStatus(UpdateRevisionStatusRequest request, string userId);//
        Task<int> DecideCameraReadyStatus(UpdateCameraReadyStatusRequest request, string userId);//
        Task<int> MarkCompleteRevise(MarkCompleteReviseRequest request, string userId);
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

        Task<List<UserSubmittedPaperDetailResponse>> GetSubmittedPaper(string userId, string? confId);
        Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId, string userId);

        Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases();
        Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersByReviewerId(string userId, string? confId);
        //Task<List<ConferenceWithAssignedPapersResponse>> GetAssignedPapersGroupedByConference(string userId, string? confId);
        Task<List<CameraReadyDtoDetail>> ListPendingCameraReady();
        Task<List<FullPaperDtoDetail>> ListPendingfullpaper();


        Task<List<PaperDetailResponseDTO>> GetListAllPaper();
        Task<List<UnAssignAbstractResponse>> GetUnassignAbstractList();
        Task<ToTalPaperDetailForReviewerResponse> GetPaperDetailForReviewer(string paperId, string userId);

        Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId);
        Task<LeaveWaitListResponse> LeaveWaitList(string userId, string conferenceId);
        Task<AddWaitListResponse> AddWaitList(string userId, string conferenceId);
        Task<List<ReviewerWorkItemResponse>> GetAssignedPapersDetailedAsync(string userId, string? confId);
        Task<List<AvailableCoAuthorResponse>> GetAvailableCoAuthorForInclude(string conferenceId, string userId);
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
        private readonly INotificationService _notificationService;
        private readonly IConferenceStepService _conferenceStepService;
        public PaperService(IUnitOfWork unitOfWork, IMomoService momoService, ITokenService tokenService, IOptions<ObjectStorageSettings> objectStorageSettings, IObjectStorageFileService objectStorageFileService, ITicketService ticketService, ITimeProviderService timeProviderService, INotificationService notificationService, IConferenceStepService conferenceStepService)
        {
            _unitOfWork = unitOfWork;
            _momoService = momoService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings;
            _objectStorageFileService = objectStorageFileService;
            _ticketService = ticketService;
            _timeProviderService = timeProviderService;
            _notificationService = notificationService;
            _conferenceStepService = conferenceStepService;
        }

        public async Task<int> SubmitAbstract(CreateAbstractRequest request, string userId)
        {
            var timeNow = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());
            var auditLogPaper = await _unitOfWork.AuditLogCategoryRepository.GetAuditLogCategoryByNameAsync(AuditLogActionNameEnum.Paper.GetDescription());
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng với id {userId}");
            }
            if (abstractPhase == null || pendingGlobalStatus == null || auditLogPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái");
            }
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(request.ConferenceId);
            if (conference == null)
            {
                throw new BadRequestException($"Hội nghị với id {request.ConferenceId} không tồn tại");

            }
            var activeResearchPhase = conference.ResearchConferencePhases.FirstOrDefault(rcp => rcp.IsActive == true);
            if (activeResearchPhase == null)
            {
                throw new BadRequestException("Không tìm thấy giai đoạn hiệu lực nào của hội nghị");
            }
            var existingPaper = await _unitOfWork.PaperRepository.GetPaperByRootUserAndConference(request.ConferenceId, userId);
            if (existingPaper != null)
            {
                
               throw new BadRequestException($"Bạn đã nộp báo cho hội nghị {existingPaper.Conference!.ConferenceName} vào {existingPaper.CreatedAt}");
            }

            if (dateNow < activeResearchPhase.RegistrationStartDate || dateNow > activeResearchPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Không thể nộp abstract, do ngày đăng kí nằm trong khoảng {activeResearchPhase.RegistrationStartDate} - {activeResearchPhase.RegistrationEndDate}");
            }
            var ownReviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, request.ConferenceId);
            if (ownReviewerContractFound != null)
            {
                throw new BadRequestException($"Bạn đang có hợp đồng với sự kiện này nên không thể thực hiện thanh toán");
            }


            var internalReviewRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            if (internalReviewRole == null)
            {
                throw new NotFoundException($"Không tìm thấy role trong hệ thống");
            }
            var userRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(userId, internalReviewRole.RoleId);
            if (userRole != null)
            {
                throw new BadRequestException($"Bạn không thể mua vé này vì bạn là reviewer trong hệ thống");
            }
            var paper = new Paper()
            {
                PaperId = Guid.NewGuid().ToString(),
                FullPaperId = null,
                RevisionPaperId = null,
                CameraReadyId = null,
                ConferenceId = request.ConferenceId,
                PaperPhase = abstractPhase,
                ResearchConferencePhase = activeResearchPhase,
                TicketId = null,
                CreatedAt = timeNow,
                Title = request.Title,
                Description = request.Description,
            };
            paper.PaperAuthors = new List<PaperAuthor>()
            {
                new PaperAuthor()
                {
                        UserId = userId,
                        IsPresenter = true,
                        IsRootAuthor = true,
                        PaperId =  paper.PaperId

                }
            };
            List<Notification> notificationList = new List<Notification>();
            string notiTitle = $"CoAuthor cho bài báo {request.Title}";
            string notiMessage = $"Bạn đã được thêm làm coauthor cho bài báo {request.Title} của hội nghị {conference.ConferenceName}";


            var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync(request.ConferenceId);
            if (request.CoAuthorId != null && request.CoAuthorId.Any())
            {
                foreach (var coauthorId in request.CoAuthorId)
                {
                    var coAuthor = await _unitOfWork.UserRepository.GetUserByUserId(coauthorId);
                    if (coAuthor == null)
                    {
                        throw new BadRequestException($"Coauthor với id {coauthorId} không tìm thấy");
                    }
                    if (coauthorId == userId)
                    {
                        throw new BadRequestException("Bạn không thể thêm chính mình là co-author.");
                    }

                    bool isReviewerOfConference = paperReviewers.Any(pr => pr.UserId == coauthorId);
                    var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(coauthorId, request.ConferenceId);
                    if (reviewerContractFound != null)
                    {
                        if (reviewerContractFound.IsActive == true)
                        {
                            throw new BadRequestException($"Co author với id {coauthorId} tên {reviewerContractFound.User!.FullName} đang có hợp đồng review với hội nghị {paper.Conference!.ConferenceName}");
                        }
                    }
                    if (isReviewerOfConference == true)
                    {
                        throw new BadRequestException($"Nguời dùng {coauthorId} đang là reviewer của hội nghị này, không thể thêm làm co-author.");
                    }
                    var paperAuthorObj = new PaperAuthor()
                    {
                        IsPresenter = false,
                        UserId = coauthorId,
                        PaperId = paper.PaperId,
                        IsRootAuthor = false,
                    };
                    paper.PaperAuthors.Add(paperAuthorObj);

                    var notification = new Notification()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = coauthorId,
                        Title = notiTitle,
                        Message = notiMessage,
                        Type = null,
                        CreatedAt = timeNow,
                        ReadStatus = false,
                    };
                    notificationList.Add(notification);
                    var coauthorDetail = await _unitOfWork.UserRepository.GetUserByUserId(coauthorId);
                    if (coauthorDetail != null)
                    {
                        if (coauthorDetail.FirebaseMobileFcmToken != null)
                        {
                            await _notificationService.SendMobilePushAsync(coauthorDetail.FirebaseMobileFcmToken, notiTitle, notiMessage);
                        }
                        if (coauthorDetail.FirebaseWebFcmToken != null)
                        {
                            await _notificationService.SendWebPushAsync(coauthorDetail.FirebaseWebFcmToken, notiTitle, notiMessage);
                        }
                    }
                }
            }
            string abstractFileUrl = string.Empty;
            if (request.AbstractFile != null)
            {
                if (request.AbstractFile.ContentType == null)
                {
                    throw new BadRequestException("Content type không được bỏ trống");
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
                CreatedAt = timeNow,
                Description = request.Description,
                Title = request.Title,
                ReviewAt = null,
            };
            paper.Abstract = abstractObj;

            var auditLogObj = new AuditLog()
            {
                AuditLogId = Guid.NewGuid().ToString(),
                CategoryId = auditLogPaper.CategoryId,
                CreatedAt = timeNow,
                UserId = userId,
                ActionDescription = $"Người dùng {user.FullName} đã {AuditLogDescriptionData.SUBMIT_ABSTRACT} cho hội nghị {conference.ConferenceName}",
            };
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int finalResult = 0;
                if (notificationList.Any())
                {
                    finalResult += await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(notificationList);
                }
                finalResult += await _unitOfWork.PaperRepository.CreatePaperAsync(paper);
                finalResult += await _unitOfWork.AuditLogRepository.CreateAuditLogAsync(auditLogObj);

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
            var timeNow = await _timeProviderService.GetVietnamTime();
            if (request.GlobalStatus.Equals(GlobalStatusEnum.Pending))
            {
                throw new BadRequestException($"Không thể chuyển pending cho abstract");
            }
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

            var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription());
            var abstractPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription());

            if (abstractPaperPhase == null || pendingGlobalStatus == null || rejectedGlobalStatus == null || acceptedGlobalStatus == null || fullPaperPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (basePaper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với id {request.PaperId} ");
            }
            var activeCurrentPhase = basePaper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giao đoạn cho hội nghị {basePaper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.AbstractDecideStatusStart || dateNow > activeCurrentPhase.AbstractDecideStatusEnd)
            {
                throw new BadRequestException($"Ngày quyết định abstract này từ {activeCurrentPhase.AbstractDecideStatusStart.ToString()} đến {activeCurrentPhase.AbstractDecideStatusEnd.ToString()}");
            }
            if (basePaper.PaperPhaseId != abstractPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper đang không trong quá trình quyết định abstract");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(request.AbstractId);
            if (abstractPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy abstract {request.AbstractId}");
            }
            if (abstractPaper.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"abstract không trong quá trình pending");
            }
            var rootAuthor = basePaper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
            string notiTitle = "Kết quả bài báo";
            string notiMessage = string.Empty;
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        abstractPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        abstractPaper.ReviewAt = timeNow;
                        abstractPaper.Reason = request.Reason;
                        basePaper.PaperPhaseId = fullPaperPhase.PaperPhaseId;

                        notiMessage = $"Bài báo với id {basePaper.PaperId} tựa đề {basePaper.Title} của bạn đã được chấp nhận ở phase abstract vào lúc {timeNow.ToString()}";

                        break;
                    case GlobalStatusEnum.Rejected:
                        abstractPaper.GlobalStatusId = rejectedGlobalStatus.GlobalStatusId;
                        abstractPaper.ReviewAt = timeNow;
                        abstractPaper.Reason = request.Reason;
                        

                        notiMessage = $"Bài báo với id {basePaper.PaperId} tựa đề {basePaper.Title} của bạn đã bị từ chối ở phase abstract vào lúc {timeNow.ToString()}";

                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }

                var notification = new Notification()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserId = rootAuthor!.UserId,
                    Title = notiTitle,
                    Message = notiMessage,
                    Type = null,
                    CreatedAt = timeNow,
                    ReadStatus = false,
                };
                var userDetail = await _unitOfWork.UserRepository.GetUserByUserId(rootAuthor.UserId);
                if (!string.IsNullOrWhiteSpace(userDetail!.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(userDetail.FirebaseMobileFcmToken, notiTitle, notiMessage);
                }
                if (!string.IsNullOrWhiteSpace(userDetail.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(userDetail.FirebaseWebFcmToken, notiTitle, notiMessage);
                }


                result += await _unitOfWork.AbstractRepository.UpdateAbstractAsync(abstractPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(basePaper);
                result += await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);
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
                throw new NotFoundException($"Không thấy trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không thấy paper với id: {request.PaperId} cho user {userId} hiện tại");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai đoạn fullpaper diễn ra từ {activeCurrentPhase.FullPaperStartDate} đến {activeCurrentPhase.FullPaperEndDate}");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không trong trạng thái full paper");
            }
            if (paper.FullPaperId != null)
            {
                throw new BadRequestException($"Full paper file đã tồn tại");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không sỡ hữu bài báo này");
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
                throw new BadRequestException("Không thể chuyển qua status pending.");
            }
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var rejectedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
            var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());
            var reviseStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Revise.GetDescription());


            var currentFullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            var revisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobal = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();

            if (pendingReviewStatus == null || rejectedReviewStatus == null || acceptedReviewStatus == null || reviseStatus == null || currentFullPaperPhase == null || cameraReadyPhase == null || revisePhase == null || pendingGlobal == null)
            {
                throw new NotFoundException($"Không thấy các trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không tìm thấy paper với id {request.PaperId}.");
            }

            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn nào cho hội nghị {paper.Conference.ConferenceName}");
            }
            if (dateNow < activeCurrentPhase.FullPaperDecideStatusStart || dateNow > activeCurrentPhase.FullPaperDecideStatusEnd)
            {
                throw new BadRequestException($"Giai đoạn review cho bài báo diễn ra từ {activeCurrentPhase.FullPaperDecideStatusStart} đến {activeCurrentPhase.FullPaperDecideStatusEnd}");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(request.FullPaperId);
            if (fullPaper == null)
            {
                throw new BadRequestException($"Full paper với id {request.FullPaperId} không tìm thấy");
            }
            if (fullPaper.FullPaperId != paper.FullPaperId)
            {
                throw new BadRequestException($"Full paper với id {request.FullPaperId} không khớp với fullpaper id của paper");

            }
            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper không trong trạng thái pending");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phase không đang trong giai đoạn full paper");
            }
            var paperReviewerList = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            if (paperReviewerList == null || paperReviewerList.Count <= 0)
            {
                throw new NotFoundException($"Không tìm thấy danh sách paper reviewer");
            }
            var headPaperReviewer = paperReviewerList.FirstOrDefault(x => x.IsHeadReviewer == true && x.UserId == userId);
            if (headPaperReviewer == null)
            {
                throw new NotFoundException($"Bạn không phải là head reviewer.");
            }
            var fullPaperReviews = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewsByFullPaperIdAsync(paper.FullPaperId);
            if (!fullPaperReviews.Any())
            {
                throw new BadRequestException($"Cần ít nhất 1 review từ các reviewer để quyết định trạng thái.");

            }
            int result = 0;
            string notiTitle = "Kết quả bài báo";
            string notiMessage = string.Empty;
            var rootAuthor = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (request.ReviewStatus)
                {
                    case ReviewStatusEnum.Accepted:

                        fullPaper.ReviewStatusId = acceptedReviewStatus.ReviewStatusId;
                        fullPaper.ReviewAt = timeNow;
                        fullPaper.Reason = request.Reason;
                        paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;
                        notiMessage = $"Bài báo với id {paper.PaperId} tựa đề {paper.Title} của bạn đã được chấp nhận trong phase fullpaper vào lúc {timeNow.ToString()}";


                        break;
                    case ReviewStatusEnum.Rejected:


                        fullPaper.ReviewStatusId = rejectedReviewStatus.ReviewStatusId;
                        fullPaper.ReviewAt = timeNow;
                        fullPaper.Reason = request.Reason;

                        notiMessage = $"Bài báo với id {paper.PaperId} tựa đề {paper.Title} của bạn đã bị từ chối trong phase fullpaper vào lúc {timeNow.ToString()}";

                        break;
                    case ReviewStatusEnum.Revise:



                        fullPaper.ReviewStatusId = reviseStatus.ReviewStatusId;
                        fullPaper.ReviewAt = timeNow;
                        fullPaper.Reason = request.Reason;
                        paper.PaperPhaseId = revisePhase.PaperPhaseId;
                        notiMessage = $"Bài báo với id {paper.PaperId} tựa đề {paper.Title} của bạn đã được chuyển sang phase revise vào lúc {timeNow.ToString()}";

                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }
                var notification = new Notification()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserId = rootAuthor!.UserId,
                    Title = notiTitle,
                    Message = notiMessage,
                    Type = null,
                    CreatedAt = timeNow,
                    ReadStatus = false,
                };
                var userDetail = await _unitOfWork.UserRepository.GetUserByUserId(rootAuthor.UserId);
                if (!string.IsNullOrWhiteSpace(userDetail!.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(userDetail.FirebaseMobileFcmToken, notiTitle, notiMessage);
                }
                if (!string.IsNullOrWhiteSpace(userDetail.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(userDetail.FirebaseWebFcmToken, notiTitle, notiMessage);
                }
                result += await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);
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
                throw new NotFoundException($"Không thấy trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm thấy trong hệ thống");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper không đang trong giai đoạn revise");
            }


            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không sở hữu bài báo này");
            }

            string revisionDeadlineId = string.Empty;
            var researchConferencePhasesFound = paper.ResearchConferencePhase;
            if (researchConferencePhasesFound == null)
            {
                throw new NotFoundException("Không tìm thấy các giai đoạn cho hội nghị nghiên cứu");
            }
            var researchConferenceDeadLine = researchConferencePhasesFound.RevisionRoundDeadlines;

            var validRevisionDeadline = researchConferenceDeadLine.FirstOrDefault(rcd => rcd.StartSubmissionDate <= dateNow && dateNow <= rcd.EndSubmissionDate);
            if (validRevisionDeadline == null)
            {
                throw new NotFoundException("Không tìm thấy deadline hợp lệ");
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
                        RevisionRound = null,
                        GlobalStatusId = pendingGlobalStatus.GlobalStatusId,
                        CreatedAt = await _timeProviderService.GetVietnamTime(),
                        ReviewAt = null,
                    };
                    paper.RevisionPaperId = revisionPaper.RevisionPaperId;
                    await _unitOfWork.RevisionPaperRepository.CreateRevisionPaperAsync(revisionPaper);
                    await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                    revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(revisionPaper.RevisionPaperId);
                }
                else
                {
                    revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
                    if (revisionPaper == null)
                    {
                        throw new BadRequestException($"Revision paper id {paper.RevisionPaperId} không tìm thấy trong hệ thống");
                    }
                    //revisionPaper.RevisionRound = revisionPaper.RevisionRound + 1;
                    if (!string.IsNullOrEmpty(revisionDeadlineId))
                    {
                        var revisionPaperSubmissionFound = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByRevisionPaperIdAndDeadlineId(paper.RevisionPaperId, revisionDeadlineId);
                        if (revisionPaperSubmissionFound != null)
                        {
                            throw new BadRequestException($"Bạn đã nộp revision, deadline hiện tại diễn ra từ {revisionPaperSubmissionFound.RevisionDeadlineRound?.StartSubmissionDate} đến {revisionPaperSubmissionFound.RevisionDeadlineRound?.EndSubmissionDate} này ");
                        }
                    }
                }
                revisionPaper.RevisionRound = validRevisionDeadline.RoundNumber;
                //var totalRevisionRoundAllowed = paper.Conference!.ResearchConferenceDetail!.RevisionAttemptAllowed;
                //if (revisionPaper.RevisionRound > totalRevisionRoundAllowed)
                //{
                //    throw new BadRequestException($"Không thể nộp thêm revision vì quá số lần: {totalRevisionRoundAllowed} cho phép, vui lòng chờ đợi head reviewer!");
                //}

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
                throw new NotFoundException($"Không tìm thấy paper với id {request.PaperId}");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn gửi feedback revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper submission id {request.RevisionPaperSubmissionId}");
            }
            var revisionPaperSubmissionDeadLine = revisionPaperSubmission.RevisionDeadlineRound;
            if (revisionPaperSubmissionDeadLine == null)
            {
                throw new NotFoundException($"Không tìm thấy revision deadline");
            }
            if (dateNow < revisionPaperSubmissionDeadLine.StartSubmissionDate || dateNow > revisionPaperSubmissionDeadLine.EndSubmissionDate)
            {
                throw new BadRequestException($"Deadline cho tương tác qua lại nằm từ {revisionPaperSubmissionDeadLine.StartSubmissionDate} đến {revisionPaperSubmissionDeadLine.EndSubmissionDate} ");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Không tìm thấy user id {userId} trong hệ thống assign cho bài báo {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException("Chức năng này dành cho head reviewer");

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
                throw new BadRequestException("Responses không được để trống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper  id {request.PaperId}");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn gửi response revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm thấy revision submission id {request.RevisionPaperSubmissionId} ");
            }
            var revisionPaperSubmissionDeadLine = revisionPaperSubmission.RevisionDeadlineRound;
            if (revisionPaperSubmissionDeadLine == null)
            {
                throw new NotFoundException($"Không tìm thấy revision deadline");
            }
            if (dateNow < revisionPaperSubmissionDeadLine.StartSubmissionDate || dateNow > revisionPaperSubmissionDeadLine.EndSubmissionDate)
            {
                throw new BadRequestException($"Deadline cho lần tương tác nằm từ {revisionPaperSubmissionDeadLine.StartSubmissionDate} đến {revisionPaperSubmissionDeadLine.EndSubmissionDate} ");
            }

            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);

            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không sỡ hữu bài báo này");
            }
            var feedBackList = new List<RevisionSubmissionFeedback>();
            foreach (var response in request.Responses)
            {
                var revisionSubmissionFeedback = await _unitOfWork.RevisionSubmissionFeedbackRepository.GetFeedbackByIdAsync(response.RevisionSubmissionFeedbackId);
                if (revisionSubmissionFeedback == null)
                {
                    throw new NotFoundException($"Không tìm thấy paper id {response.RevisionSubmissionFeedbackId} trong hệ thống");
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
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var rejectGlobalStautus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());


            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            if (acceptedGlobalStatus == null || currentRevisePhase == null || pendingGlobalStatus == null || rejectGlobalStautus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
            }

            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper  id {request.PaperId}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            //if (dateNow < activeCurrentPhase.RevisionPaperReviewStart || dateNow > activeCurrentPhase.RevisionPaperReviewEnd)
            //{
            //    throw new BadRequestException($"Giai đoạn gửi review revise diễn ra từ {activeCurrentPhase.RevisionPaperReviewStart} đến {activeCurrentPhase.RevisionPaperReviewEnd}");
            //}


            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không thể review vì paper đang không trong giai đoạn revise");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);

            if (paperReviewer == null)
            {
                throw new BadRequestException($"Bạn không có trong danh sách gán review");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper {request.RevisionPaperId} trong hệ thống");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm thấy revision paper {request.RevisionPaperId} tương ứng với paper");
            }
            if (revisionPaper.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision này dang không trong trong trạng thái pending");
            }
            var revisionReview = await _unitOfWork.RevisionPaperReviewRepository.GetRevisionPaperReviewByRevisionPaperAndUserAsync(paper.RevisionPaperId, userId);
            if (revisionReview != null)
            {
                throw new BadRequestException($"Bạn đã nộp revision review cho bài báo này rồi");
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
                //FeedbackToAuthor = request.FeedbackToAuthor,
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
                throw new BadRequestException("Không thể chuyển pending");
            }
            if (currentRevisePhase == null || pendingGlobalStatus == null || acceptedGlobalStatus == null || cameraReadyPaperPhase == null || rejectGlobalStautus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper {request.PaperId}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn nào diễn ra cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RevisionPaperDecideStatusStart || dateNow > activeCurrentPhase.RevisionPaperDecideStatusEnd)
            {
                throw new BadRequestException($"Giai đoạn quyết định revise diễn ra từ {activeCurrentPhase.RevisionPaperDecideStatusStart} đến {activeCurrentPhase.RevisionPaperDecideStatusEnd}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper không trong giai đoạn revise");
            }
            //dùng hàm get
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy  revision paper {request.RevisionPaperId} ");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Paper {request.PaperId} không thuộc revision paper {request.RevisionPaperId}");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Bạn không có quyền hạn quyết định bài báo này");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new BadRequestException($"Bạn không phải head reviewer");
            }

            //var revisionPaperReviews = await _unitOfWork.RevisionPaperReviewRepository.GetRevisionPaperReviewByRevisionPaperIdAsync(paper.RevisionPaperId);
            //if (!revisionPaperReviews.Any())
            //{
            //    throw new BadRequestException("Cần ít nhất 1 review để quyết định trạng thái");
            //}
            bool byPassDecideRevise = false;
            var revisionPaperSubmissionCount = revisionPaper.RevisionPaperSubmissions.Count();
            var revisionSubmissionRule = paper.Conference.ResearchConferenceDetail.RevisionAttemptAllowed;
            if (revisionPaperSubmissionCount == revisionSubmissionRule)
            {
                byPassDecideRevise = true;
            }
            if (revisionPaper.RevisionRoundDeadlineId != null)
            {
                byPassDecideRevise = true;
            }
            if (!byPassDecideRevise)
            {
                throw new BadRequestException($"Người sỡ hữu bài báo đã đi hết {revisionPaperSubmissionCount}," +
                    $" họ phải đi hết revision round " +
                    $"đã quy định trong" +
                    $" conference: {paper.Conference.ResearchConferenceDetail.RevisionAttemptAllowed} hoặc bạn có thể mark complete nếu ưng ý tại vào nào");
            }




            var rootAuthor = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
            var timeNow = await _timeProviderService.GetVietnamTime();
            string notiTitle = "Kết quả bài báo";
            string notiMessage = string.Empty;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = 0;
                switch (request.GlobalStatus)
                {
                    case GlobalStatusEnum.Accepted:
                        //update instance get:
                        revisionPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        revisionPaper.ReviewAt = timeNow;
                        revisionPaper.Reason = request.Reason;
                        paper.PaperPhaseId = cameraReadyPaperPhase.PaperPhaseId;

                        notiMessage = $"Bài báo với id {paper.PaperId} tựa đề {paper.Title} của bạn đã được chấp nhận trong phase camera ready vào lúc {timeNow.ToString()}";



                        break;

                    case GlobalStatusEnum.Rejected:
                        revisionPaper.GlobalStatusId = rejectGlobalStautus.GlobalStatusId;
                        revisionPaper.ReviewAt = timeNow;
                        revisionPaper.Reason = request.Reason;

                        notiMessage = $"Bài báo với id {paper.PaperId} tựa đề {paper.Title} của bạn đã bị từ chối trong phase camera ready vào lúc {timeNow.ToString()}";

                        break;

                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }

                var notification = new Notification()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserId = rootAuthor!.UserId,
                    Title = notiTitle,
                    Message = notiMessage,
                    Type = null,
                    CreatedAt = timeNow,
                    ReadStatus = false,
                };
                var userDetail = await _unitOfWork.UserRepository.GetUserByUserId(rootAuthor.UserId);
                if (!string.IsNullOrWhiteSpace(userDetail!.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(userDetail.FirebaseMobileFcmToken, notiTitle, notiMessage);
                }
                if (!string.IsNullOrWhiteSpace(userDetail.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(userDetail.FirebaseWebFcmToken, notiTitle, notiMessage);
                }

                //call hàm update
                result += await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                result += await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);

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
                throw new NotFoundException($"Không tìm thấy  paper {request.PaperId} g");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Bạn không có quyền truy cập bài báo này");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Không tìm thấy revision paper {request.RevisionPaperId} trong paper {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException($"Bạn không phải head reviewer");
            }
            var listRevisionPaperReview = await _unitOfWork.RevisionPaperReviewRepository.GetRevisionPaperReviewByRevisionPaperIdAsync(request.RevisionPaperId);
            var listRevisionPaperReviewResponse = listRevisionPaperReview.Select(x => new RevisionPaperReviewResponse
            {
                RevisionPaperReviewId = x.RevisionPaperReviewId,
                GlobalStatusId = x.GlobalStatusId,
                GlobalStatusName = x.GlobalStatus?.Name,
                Note = x.Note,
                CreatedAt = x.CreatedAt,
                //FeedbackToAuthor = x.FeedbackToAuthor,
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
                throw new BadRequestException($"Bài báo với id {request.PaperId} không tồn tại.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.CameraReadyStartDate || dateNow > activeCurrentPhase.CameraReadyEndDate)
            {
                throw new BadRequestException($"Giai đoạn {activeCurrentPhase.CameraReadyStartDate} đến {activeCurrentPhase.CameraReadyEndDate}");
            }

            // Check if paper already has a camera ready
            if (!string.IsNullOrEmpty(paper.CameraReadyId))
            {
                throw new BadRequestException($"bài báo với mã {request.PaperId} đã có camera ready.");
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
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
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
                throw new BadRequestException("paper phải có revision hoặc fullpaper chấp nhận mới nộp được camera ready.");
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
                throw new BadRequestException($"Camera ready với id {request.CameraReadyId} không tồn tại.");
            }

            // Validate that the camera ready is in "Pending" status
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (cameraReady.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Camera ready phải trong trạng thái pending để cập nhật.");
            }

            // Find the paper associated with this camera ready
            var paper = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(request.CameraReadyId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper liên kết với camera ready ID {request.CameraReadyId} không tồn tại.");
            }
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paper.PaperId);
            if (paperAuthors == null)
            {
                throw new NotFoundException("Không tìm thấy paper author nào");
            }
            var paperOwnerShip = paperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (paperOwnerShip == null)
            {
                throw new BadRequestException("Bài báo không thuộc quyền sỡ hữu của bạn");
            }

            cameraReady.Title = string.IsNullOrWhiteSpace(request.Title) ? cameraReady.Title : request.Title;
            cameraReady.Description = string.IsNullOrWhiteSpace(request.Description) ? cameraReady.Description : request.Description;

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
                throw new BadRequestException($"User với id {userId} không tồn tại.");
            }
            if (request.reviewStatus == ReviewStatusEnum.Pending)
            {
                throw new BadRequestException("Không thể thành pending cho. Chỉ được accept hoặc reject");
            }
            var timeNow = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
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
                throw new BadRequestException($"Full paper với id {request.FullPaperId} không tồn tại.");
            }

            // Validate that the user is assigned as a reviewer to this paper
            var paper = await _unitOfWork.PaperRepository.GetPaperByFullPaperIdAsync(request.FullPaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Bài báo với full paper ID {request.FullPaperId} không tồn tại.");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paper.PaperId);
            var activeCurrentPhase = basePaper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn cho hội nghị {paper.Conference!.ConferenceName}");
            }

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
            if (paperReviewer == null)
            {
                throw new BadRequestException($"Bạn hiện tại không tìm thấy trong danh sách gán reviewer");
            }







            // Check if the user has already submitted a review for this full paper
            var existingReview = await _unitOfWork.FullPaperReviewRepository.GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(request.FullPaperId, userId);
            if (existingReview != null)
            {
                throw new BadRequestException("Bạn đã gửi full paper review rồi.");
            }
            if (dateNow < activeCurrentPhase.ReviewStartDate || dateNow > activeCurrentPhase.ReviewEndDate)
            {
                throw new BadRequestException($"Giai đoạn gửi full paper review nằm từ {activeCurrentPhase.ReviewStartDate} đến {activeCurrentPhase.ReviewEndDate}.");

            }

            // Validate that the full paper is in "Pending" review status
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            if (pendingReviewStatus == null)
            {
                throw new BadRequestException("Không tìm thấy trạng thái trong hệ thống");
            }

            if (fullPaper.ReviewStatusId != pendingReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException("Full paper phải trong trạng thái pending.");
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
                CreatedAt = timeNow,
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
                throw new BadRequestException($"Camera ready với id {request.CameraReadyId} không tồn tại.");
            }

            // Validate that the camera ready is in "Pending" status
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingGlobalStatus == null)
            {
                throw new BadRequestException("Giai đoạn pending không tồn tại trong hệ thống");
            }

            if (cameraReady.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Camera ready phải trong trạng thái pending");
            }

            // Validate that the user is a head reviewer for the paper associated with this camera ready
            var timeNow = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
            var paper = await _unitOfWork.PaperRepository.GetPaperByCameraReadyIdAsync(request.CameraReadyId);
            if (paper == null)
            {
                throw new BadRequestException($"Bài báo với camera id {request.CameraReadyId} không tồn tại hoặc liên kết với nhau.");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paper.PaperId);

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
            if (paperReviewer == null)
            {
                throw new BadRequestException("Bạn không có quyền trong bài báo này");
            }
            var activeCurentResearchPhase = basePaper.ResearchConferencePhase;
            if (activeCurentResearchPhase == null)
            {
                throw new NotFoundException("Không tìm thấy giai đoạn research trong bài báo");

            }
            if (dateNow < activeCurentResearchPhase.CameraReadyDecideStatusStart || dateNow > activeCurentResearchPhase.CameraReadyDecideStatusEnd)
            {
                throw new BadRequestException($"Giai đoạn quyết định camera ready từ {activeCurentResearchPhase.CameraReadyDecideStatusStart} đến {activeCurentResearchPhase.CameraReadyDecideStatusEnd}");
            }
            if (paperReviewer.IsHeadReviewer != true)
            {
                throw new BadRequestException("Chỉ head reviewer mới có thể quyết định bài báo");
            }
            // Update the camera ready status based on the request
            string notiTitle = "Kết quả bài báo";
            string notiMessage = string.Empty;
            var rootAuthor = basePaper!.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true);
            GlobalStatus? newGlobalStatus = null;
            switch (request.GlobalStatus)
            {
                case GlobalStatusEnum.Accepted:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    
                    notiMessage = $"Bài báo với id {basePaper.PaperId} tựa đề {basePaper.Title} của bạn đã được chấp nhận trong phase camera ready vào lúc {timeNow.ToString()}";


                    break;
                case GlobalStatusEnum.Rejected:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

                    

                    notiMessage = $"Bài báo với id {basePaper.PaperId} tựa đề {basePaper.Title} của bạn đã bị từ chối trong phase camera ready vào lúc {timeNow.ToString()}";


                    break;
                case GlobalStatusEnum.Pending:
                    newGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
                    break;
                default:
                    throw new BadRequestException("Status không khả dụng");
            }

            if (newGlobalStatus == null)
            {
                throw new BadRequestException($"{request.GlobalStatus.GetDescription()} không tồn tại.");
            }
            int result = 0;
            cameraReady.GlobalStatusId = newGlobalStatus.GlobalStatusId;
            cameraReady.ReviewAt = timeNow;
            cameraReady.Reason = request.Reason;
            var notification = new Notification()
            {
                NotificationId = Guid.NewGuid().ToString(),
                UserId = rootAuthor!.UserId,
                Title = notiTitle,
                Message = notiMessage,
                Type = null,
                CreatedAt = timeNow,
                ReadStatus = false,
            };
            var userDetail = await _unitOfWork.UserRepository.GetUserByUserId(rootAuthor.UserId);
            if (!string.IsNullOrWhiteSpace(userDetail!.FirebaseMobileFcmToken))
            {
                await _notificationService.SendMobilePushAsync(userDetail.FirebaseMobileFcmToken, notiTitle, notiMessage);
            }
            if (!string.IsNullOrWhiteSpace(userDetail.FirebaseWebFcmToken))
            {
                await _notificationService.SendWebPushAsync(userDetail.FirebaseWebFcmToken, notiTitle, notiMessage);
            }
            result += await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);
            result += await _unitOfWork.CameraReadyRepository.UpdateCameraReadyAsync(cameraReady);
            return result;
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

        public async Task<List<UserSubmittedPaperDetailResponse>> GetSubmittedPaper(string userId, string? confId)
        {
            // Use the new repository method to get papers by user ID in a single query
            var submittedPapers = await _unitOfWork.PaperAuthorRepository.GetPapersByUserIdAsync(userId);
            if (confId != null)
                submittedPapers = submittedPapers.Where(p => p.ConferenceId == confId).ToList();

            return submittedPapers.Select(p => new UserSubmittedPaperDetailResponse
            {
                PaperId = p.PaperId,
                AbstractId = p.AbstractId,
                FullPaperId = p.FullPaperId,
                RevisionPaperId = p.RevisionPaperId,
                CameraReadyId = p.CameraReadyId,


                ConferenceId = p.ConferenceId,
                ConferenceName = p.Conference?.ConferenceName,
                ConferenceDescription = p.Conference?.Description,
                ConferenceStartDate = p.Conference?.StartDate,
                ConferenceEndDate = p.Conference?.EndDate,
                ConferenceTotalSlot = p.Conference?.TotalSlot,
                ConferenceAvailableSlot = p.Conference?.AvailableSlot,
                Address = p.Conference?.Address,
                BannerImageUrl = p.Conference?.BannerImageUrl,
                ConferenceCreatedAt = p.Conference?.CreatedAt,
                TicketSaleStart = p.Conference?.TicketSaleStart,
                TicketSaleEnd = p.Conference?.TicketSaleEnd,
                IsInternalHosted = p.Conference?.IsInternalHosted,
                IsResearchConference = p.Conference?.IsResearchConference,
                CityId = p.Conference?.CityId,
                CityName = p.Conference?.City?.CityName,

                ConferenceCreatedBy = p.Conference?.CreatedBy,
                ConferenceCreatedByEmail = p.Conference?.CreatedByNavigation?.Email,
                ConferenceCreatedByFullName = p.Conference?.CreatedByNavigation?.FullName,
                ConferenceCreatedByAvatarUrl = p.Conference?.CreatedByNavigation?.AvatarUrl,

                ConferenceCategoryId = p.Conference?.ConferenceCategoryId,
                ConferenceStatusId = p.Conference?.ConferenceCategory?.ConferenceCategoryName,

                PaperPhaseId = p.PaperPhaseId,
                PhaseName = p.PaperPhase?.PhaseName,
                ResearchConferencePhaseId = p.ResearchConferencePhaseId,

                TicketId = p.TicketId,

                PaperCreatedAt = p.CreatedAt,
                PaperTitle = p.Title,
                PaperDescription = p.Description,


                Abstract = p.Abstract == null ? null : new UserSubmittedAbstract
                {
                    AbstractId = p.Abstract.AbstractId,
                    AbstractUrl = p.Abstract.AbstractUrl,
                    Title = p.Abstract.Title,
                    Description = p.Abstract.Description,
                    CreatedAt = p.Abstract.CreatedAt,
                    ReviewAt = p.Abstract.ReviewAt,
                    GlobalStatusId = p.Abstract.GlobalStatusId,
                    GlobalStatusName = p.Abstract.GlobalStatus?.Name

                },

                FullPaper = p.FullPaper == null ? null : new UserSubmittedFullPaper
                {
                    FullPaperId = p.FullPaper.FullPaperId,
                    FullPaperUrl = p.FullPaper.FullPaperUrl,
                    Title = p.FullPaper.Title,
                    Description = p.FullPaper.Description,
                    CreatedAt = p.FullPaper.CreatedAt,
                    ReviewAt = p.FullPaper.ReviewAt,
                    ReviewStatusId = p.FullPaper.ReviewStatusId,
                    ReviewStatusName = p.FullPaper.ReviewStatus?.Name,

                },


                RevisionPaper = p.RevisionPaper == null ? null : new UserSubmittedRevisionPaper
                {
                    RevisionPaperId = p.RevisionPaper.RevisionPaperId,
                    RevisionRound = p.RevisionPaper.RevisionRound,
                    GlobalStatusId = p.RevisionPaper.GlobalStatusId,
                    GlobalStatusName = p.RevisionPaper.GlobalStatus?.Name,
                    CreatedAt = p.RevisionPaper.CreatedAt,
                    ReviewAt = p.RevisionPaper.ReviewAt,

                    RevisionRoundDeadlineId = p.RevisionPaper.RevisionRoundDeadlineId,
                    RevisionRoundDeadlineStartSubmissionDate =
              p.RevisionPaper.RevisionRoundDeadline?.StartSubmissionDate,
                    RevisionRoundDeadlineEndSubmissionDate =
              p.RevisionPaper.RevisionRoundDeadline?.EndSubmissionDate,
                    RevisionRoundDeadlineRoundNumber =
              p.RevisionPaper.RevisionRoundDeadline?.RoundNumber
                },

                CameraReady = p.CameraReady == null ? null : new UserSubmittedCameraReady
                {
                    CameraReadyId = p.CameraReady.CameraReadyId,
                    CameraReadyUrl = p.CameraReady.CameraReadyUrl,
                    Title = p.CameraReady.Title,
                    Description = p.CameraReady.Description,
                    CreatedAt = p.CameraReady.CreatedAt,
                    ReviewAt = p.CameraReady.ReviewAt,
                    GlobalStatusId = p.CameraReady.GlobalStatusId,
                    GlobalStatusName = p.CameraReady.GlobalStatus?.Name,

                }

            }).ToList();
        }

        public async Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId, string userId)
        {
            // Step 1: Fetch the main Paper entity. This is our starting point.
            // We get Phase and CameraReady here because they are included in the repo method.
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdWithPhaseAsync(paperId);

            if (paper == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy paper với id {paperId}");
            }

            var paperAuthor = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
            var authorIds = paperAuthor.Select(pa => pa.UserId).ToList();

            if (!authorIds.Contains(userId))
                throw new Exception("Bạn không thuộc tác giả của bài báo này, không thể xem chi tiết");

            var researchConferencePhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByPaperId(paper.PaperId);
            if (researchConferencePhase == null) throw new BadRequestException("Paper này chưa có research phase");
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
                RootAuthor = RootAuthor != null ? new Author { userId = RootAuthor.UserId, fullName = RootAuthor.FullName, avatarUrl = RootAuthor.AvatarUrl } : null,
                CoAuthors = coAuthors?.Select(user => new Author
                {
                    userId = user.UserId,
                    fullName = user.FullName,
                    avatarUrl = user.AvatarUrl
                }).ToList(),
                researchConferenceInfo = await _conferenceStepService.GetResearchConferenceBasicAsync(paper.ConferenceId),
                ResearchPhase = paper.ResearchConferencePhase != null ? new ResearchPhaseDtoDetail
                {
                    ResearchConferencePhaseId = paper.ResearchConferencePhase.ResearchConferencePhaseId,
                    ConferenceId = paper.ConferenceId,

                    // 1. Registration
                    RegistrationStartDate = paper.ResearchConferencePhase.RegistrationStartDate,
                    RegistrationEndDate = paper.ResearchConferencePhase.RegistrationEndDate,

                    // 2. Abstract Decide
                    AbstractDecideStatusStart = paper.ResearchConferencePhase.AbstractDecideStatusStart,
                    AbstractDecideStatusEnd = paper.ResearchConferencePhase.AbstractDecideStatusEnd,

                    // 3. Full Paper
                    FullPaperStartDate = paper.ResearchConferencePhase.FullPaperStartDate,
                    FullPaperEndDate = paper.ResearchConferencePhase.FullPaperEndDate,

                    // 4. Review
                    ReviewStartDate = paper.ResearchConferencePhase.ReviewStartDate,
                    ReviewEndDate = paper.ResearchConferencePhase.ReviewEndDate,

                    // 5. Full Paper Decide
                    FullPaperDecideStatusStart = paper.ResearchConferencePhase.FullPaperDecideStatusStart,
                    FullPaperDecideStatusEnd = paper.ResearchConferencePhase.FullPaperDecideStatusEnd,

                    // 6. Revise
                    ReviseStartDate = paper.ResearchConferencePhase.ReviseStartDate,
                    ReviseEndDate = paper.ResearchConferencePhase.ReviseEndDate,

                    // 7. Revision Review
                    //RevisionPaperReviewStart = paper.ResearchConferencePhase.RevisionPaperReviewStart,
                    //RevisionPaperReviewEnd = paper.ResearchConferencePhase.RevisionPaperReviewEnd,

                    // 8. Revision Decide
                    RevisionPaperDecideStatusStart = paper.ResearchConferencePhase.RevisionPaperDecideStatusStart,
                    RevisionPaperDecideStatusEnd = paper.ResearchConferencePhase.RevisionPaperDecideStatusEnd,

                    // 9. Camera Ready
                    CameraReadyStartDate = paper.ResearchConferencePhase.CameraReadyStartDate,
                    CameraReadyEndDate = paper.ResearchConferencePhase.CameraReadyEndDate,

                    // 10. Camera Ready Decide
                    CameraReadyDecideStatusStart = paper.ResearchConferencePhase.CameraReadyDecideStatusStart,
                    CameraReadyDecideStatusEnd = paper.ResearchConferencePhase.CameraReadyDecideStatusEnd
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
                RevisionRoundDeadlineId = entity.RevisionRoundDeadlineId,
                Created = entity.CreatedAt,
                Updated = entity.ReviewAt,
                OverallStatus = entity.GlobalStatus?.Name,
                Reviews = entity.RevisionPaperReviews?.Select(review => new RevisionReviewDtoDetail
                {
                    ReviewId = review.RevisionPaperReviewId,
                    Note = review.Note,
                    //FeedBackToAuthor = review.FeedbackToAuthor,
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
                throw new NotFoundException("Không tìm thấy trạng thái");
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

        public async Task<ToTalPaperDetailForReviewerResponse?> GetPaperDetailForReviewer(string paperId, string userId)
        {
            var paperReviewerCheck = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paperId);
            if (paperReviewerCheck == null)
            {
                throw new BadRequestException("Bạn không có quyền hạn xem paper này");

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
                throw new BadRequestException($"Hội nghị {conferenceId} không tồn tại");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound == null)
            {
                throw new BadRequestException($"Không tồn tại hàng đợi");
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
                throw new BadRequestException($"Hội nghị id {conferenceId} không tồn tại");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound != null)
            {
                throw new BadRequestException($"Bạn đang trong hàng đợi rồi");
            }
            var paperWaitListNotifiedStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());
            if (paperWaitListNotifiedStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
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
                throw new NotFoundException($"Không tìm thấy trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với mã {request.PaperId}");
            }
            if (paper.AbstractId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chưa có abstract để chỉnh sửa");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId);
            if (abstractPaper!.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Abstract hiện không pending để sửa.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Giai đoạn sửa abstract diễn ra từ {activeCurrentPhase.RegistrationStartDate} đến {activeCurrentPhase.RegistrationEndDate}");
            }

            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại đang không trong giai đoạn chỉnh sửa abstract");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không sỡ hữu bài báo này");
            }
            var conferenceReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync(paper.ConferenceId);
            string notiTitle = $"CoAuthor cho bài báo {request.Title}";
            string notiMessage = $"Bạn đã được thêm làm coauthor cho bài báo {request.Title} của hội nghị {paper.Conference!.ConferenceName}";
            var timeNow = await _timeProviderService.GetVietnamTime();
            List<PaperAuthor> paperAuthorList = new List<PaperAuthor>();
            List<Notification> notificationList = new List<Notification>();
            if (request.CoAuthorId != null && request.CoAuthorId.Any() && conferenceReviewers.Count() > 0)
            {
                foreach (var coauthorId in request.CoAuthorId)
                {
                    if (coauthorId == userId)
                    {
                        throw new BadRequestException("Bạn không thể thêm mình làm coauthor.");
                    }
                    //check coauthor có là reviewer cho bài báo này
                    bool isReviewerOfConference = conferenceReviewers.Any(pr => pr.UserId == coauthorId);
                    if (isReviewerOfConference == true)
                    {
                        throw new BadRequestException($"Nguời dùng {coauthorId} đang là reviewer của hội nghị {paper.Conference?.ConferenceName}.");
                    }

                    //check coauthor có là external reviewer có contract với hội nghị
                    var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(coauthorId, paper.Conference!.ConferenceId);
                    if (reviewerContractFound != null)
                    {
                        if (reviewerContractFound.IsActive == true)
                        {
                            throw new BadRequestException($"Co author với id {coauthorId} hiện đang có hợp đồng reviewer");
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

                    var notification = new Notification()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = coauthorId,
                        Title = notiTitle,
                        Message = notiMessage,
                        Type = null,
                        CreatedAt = timeNow,
                        ReadStatus = false,
                    };
                    notificationList.Add(notification);


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
                    finalResult += await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(notificationList);
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
                throw new NotFoundException($"Không tìm thấy trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với mã {request.PaperId} ");
            }
            if (paper.FullPaperId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chưa có full paper để chỉnh sửa");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId);
            if (fullPaper!.ReviewStatusId != pendingFullPaperReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper hiện không trong thái 'Pending', nên không thể chỉnh sửa. Trạng thái hiện tại là {fullPaper.ReviewStatus?.Name}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn cho hội nghị {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai đoạn fullpaper diễn ra từ {activeCurrentPhase.FullPaperStartDate} đến {activeCurrentPhase.FullPaperEndDate}");
            }

            if (paper.PaperPhaseId != fullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không trong quá trình chỉnh sửa fullpaper");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sở hữu bài báo này");
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
                throw new NotFoundException($"Không thấy trạng thái");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm thấy");
            }
            if (paper.RevisionPaperId == null)
            {
                throw new NotFoundException($"Mã bài báo {request.PaperId} không tìm thấy revision id ");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn cho {paper.Conference!.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            //if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            //{
            //    throw new BadRequestException($"Giai đoạn revise diễn ra từ  {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            //}
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phải trong trạng thái revise");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không sở hữu bài báo này");
            }
            var revisionPaperFound = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
            if (revisionPaperFound == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper với id {paper.RevisionPaperId}");
            }
            if (revisionPaperFound.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision paper phải trong trạng thái pending để update");
            }
            var revisionPaperSubmissionsList = revisionPaperFound.RevisionPaperSubmissions;
            if (revisionPaperSubmissionsList == null || !revisionPaperSubmissionsList.Any())
            {
                throw new NotFoundException("Không tìm thấy danh sách revision paper submission");
            }
            var currentRevisionPaperSubmission = revisionPaperSubmissionsList.FirstOrDefault(rps => rps.RevisionPaperSubmissionId == request.RevisionPaperSubmissionId);
            if (currentRevisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper submission với id {request.RevisionPaperSubmissionId}");
            }
            var currentRevisionPaperSubmissionDeadline = currentRevisionPaperSubmission.RevisionDeadlineRound;
            if (currentRevisionPaperSubmissionDeadline == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin deadline của revision submission này");
            }
            if (dateNow < currentRevisionPaperSubmissionDeadline!.StartSubmissionDate || dateNow > currentRevisionPaperSubmissionDeadline!.EndSubmissionDate)
            {
                throw new BadRequestException($"Bạn không thể chỉnh sửa vì deadline revision submission này từ {currentRevisionPaperSubmissionDeadline.StartSubmissionDate} đến {currentRevisionPaperSubmissionDeadline.EndSubmissionDate}");
            }
            var revisionSubmissionFeedbackList = currentRevisionPaperSubmission.RevisionSubmissionFeedbacks;
            if (revisionSubmissionFeedbackList.Any())
            {
                throw new BadRequestException($"Bạn không thể update  vì  revision submission này vì đã có head reviewer đưa ra đánh giá. ");
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
                        throw new BadRequestException("Content type không hợp lệ");
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
        public async Task<int> MarkCompleteRevise(MarkCompleteReviseRequest request, string userId)
        {
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (currentRevisePhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper {request.PaperId}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy giai đoạn nào diễn ra cho hội nghị {paper.Conference.ConferenceName}");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn đánh dấu revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper không trong giai đoạn revise");
            }
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy  revision paper {request.RevisionPaperId} ");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Paper {request.PaperId} không thuộc revision paper {request.RevisionPaperId}");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Bạn không có quyền hạn quyết định bài báo này");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new BadRequestException($"Bạn không phải head reviewer");
            }
            var validRoundDeadLine = activeCurrentPhase.RevisionRoundDeadlines;
            if (!validRoundDeadLine.Any())
            {
                throw new NotFoundException("Không tìm thấy round deadline nào");
            }
            var roundDeadline = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByIdAsync(request.RevisionRoundDeadlineId);
            if (roundDeadline == null)
            {
                throw new BadRequestException($"Round deadline với id {request.RevisionRoundDeadlineId} không tồn tại");
            }
            if (!validRoundDeadLine.Select(rdl => rdl.RevisionRoundDeadlineId).ToHashSet().Contains(roundDeadline.RevisionRoundDeadlineId))
            {
                throw new BadRequestException("Round deadline bạn đưa không nằm trong round deadline quy định");
            }
            revisionPaper.RevisionRoundDeadline = roundDeadline;
            return await _unitOfWork.RevisionPaperRepository.UpdateRevisionPaperAsync(revisionPaper);
        }

        public async Task<List<ReviewerWorkItemResponse>> GetAssignedPapersDetailedAsync(string userId, string? confId)
        {
            // BƯỚC 1: Lấy assignments
            var assignments = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAndConferenceIdAsync(userId, confId);
            if (!assignments.Any()) return new List<ReviewerWorkItemResponse>();

            var paperIds = assignments.Select(x => x.PaperId).Distinct().ToList();

            // BƯỚC 2: Get Details (Query tối ưu)
            var papers = await _unitOfWork.PaperRepository.GetDetailPaperFromListId(paperIds);

            // BƯỚC 3: Pre-load Reviews
            var myFullPaperReviews = await _unitOfWork.FullPaperReviewRepository
                .GetReviewsByUserAndPaperIdsAsync(userId, paperIds);

            var myRevisionReviews = await _unitOfWork.RevisionPaperReviewRepository
                .GetReviewsByUserAndPaperIdsAsync(userId, paperIds);

            var dateNow = await _timeProviderService.GetVietnamDate();
            var responseList = new List<ReviewerWorkItemResponse>();
            const string PendingStatus = "Pending";

            // BƯỚC 4: Loop & Map
            foreach (var assign in assignments)
            {
                var paper = papers.FirstOrDefault(p => p.PaperId == assign.PaperId);
                if (paper == null) continue;

                var phaseConfig = paper.ResearchConferencePhase;
                bool isHead = assign.IsHeadReviewer ?? false;

                var dto = new ReviewerWorkItemResponse
                {
                    PaperId = paper.PaperId,
                    Title = paper.Title,
                    ConferenceName = paper.Conference?.ConferenceName,
                    CurrentPhaseName = paper.PaperPhase?.PhaseName,
                    IsHeadReviewer = isHead
                };

                // --- A. FULL PAPER ---
                if (paper.FullPaper != null)
                {
                    var myReview = myFullPaperReviews.FirstOrDefault(r => r.FullPaperId == paper.FullPaperId);
                    bool isFpPending = paper.FullPaper.ReviewStatus?.Name == PendingStatus;

                    dto.FullPaperWork = new FullPaperWorkItem
                    {
                        FullPaperId = paper.FullPaper.FullPaperId,
                        FileUrl = paper.FullPaper.FullPaperUrl,
                        StatusName = paper.FullPaper.ReviewStatus?.Name,

                        IsMyReviewSubmitted = myReview != null,
                        MyReviewResult = myReview?.ReviewStatus?.Name,

                        CanReview = phaseConfig != null
                                    && dateNow >= phaseConfig.ReviewStartDate
                                    && dateNow <= phaseConfig.ReviewEndDate
                                    && myReview == null,

                        CanDecide = isHead
                                    && phaseConfig != null
                                    && dateNow >= phaseConfig.FullPaperDecideStatusStart
                                    && dateNow <= phaseConfig.FullPaperDecideStatusEnd
                                    && isFpPending
                    };
                }

                // --- B. REVISION ---
                if (paper.RevisionPaper != null)
                {
                    var currentRound = paper.RevisionPaper.RevisionRound ?? 1;

                    // Tìm Deadline của Round hiện tại
                    var roundDeadline = paper.ResearchConferencePhase?.RevisionRoundDeadlines
                        .FirstOrDefault(d => d.RoundNumber == currentRound);

                    // Ưu tiên ngày của Round, nếu null thì lấy ngày của Phase
                    var startReviseDate = roundDeadline?.StartSubmissionDate ?? phaseConfig?.ReviseStartDate;
                    var endReviseDate = roundDeadline?.EndSubmissionDate ?? phaseConfig?.ReviseEndDate;

                    // --- FIX QUAN TRỌNG: Lấy submission MỚI NHẤT ---
                    // Phải order by roundnumber giảm dần để lấy file nộp sau cùng
                    var latestSub = paper.RevisionPaper.RevisionPaperSubmissions?
                        .OrderByDescending(s => s.RevisionDeadlineRound?.RoundNumber ?? 1)
                        .FirstOrDefault();

                    // ---  Check xem user đã feedback cho submission này chưa ---
                    bool hasGivenFeedback = false;
                    if (latestSub != null && latestSub.RevisionSubmissionFeedbacks != null)
                    {
                        // Kiểm tra xem có feedback nào được tạo bởi userId hiện tại không
                        hasGivenFeedback = latestSub.RevisionSubmissionFeedbacks
                            .Any(fb => fb.UserId == userId);
                    }

                    var myRevReview = myRevisionReviews.FirstOrDefault(r => r.RevisionPaperId == paper.RevisionPaperId);
                    bool isRevPending = paper.RevisionPaper.GlobalStatus?.Name == PendingStatus;

                    dto.RevisionWork = new RevisionWorkItem
                    {
                        RevisionPaperId = paper.RevisionPaper.RevisionPaperId,
                        RevisionRound = currentRound,
                        StatusName = paper.RevisionPaper.GlobalStatus?.Name,
                        LatestFileUrl = latestSub?.RevisionPaperUrl,

                        IsFeedbackSubmitted = hasGivenFeedback,

                        IsMyReviewSubmitted = myRevReview != null,

                        CanGiveFeedback = isHead
                                          && dateNow >= startReviseDate
                                          && dateNow <= endReviseDate,

                        CanDecide = isHead
                                    && isRevPending
                                    && phaseConfig != null
                                    && dateNow >= phaseConfig.RevisionPaperDecideStatusStart
                                    && dateNow <= phaseConfig.RevisionPaperDecideStatusEnd
                    };
                }

                // --- C. CAMERA READY ---
                if (paper.CameraReady != null)
                {
                    bool isCrPending = paper.CameraReady.GlobalStatus?.Name == PendingStatus;

                    dto.CameraReadyWork = new CameraReadyWorkItem
                    {
                        CameraReadyId = paper.CameraReady.CameraReadyId,
                        FileUrl = paper.CameraReady.CameraReadyUrl,
                        StatusName = paper.CameraReady.GlobalStatus?.Name,

                        CanDecide = isHead
                                    && phaseConfig != null
                                    && dateNow >= phaseConfig.CameraReadyDecideStatusStart
                                    && dateNow <= phaseConfig.CameraReadyDecideStatusEnd
                                    && isCrPending
                    };
                }

                responseList.Add(dto);
            }

            return responseList;
        }

        public async Task<List<AvailableCoAuthorResponse>> GetAvailableCoAuthorForInclude(string conferenceId, string userId)
        {
            var adminRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Admin.GetDescription());
            var organizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.ConferenceOrganizer.GetDescription());
            var internalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            var collabRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.Collaborator.GetDescription());
            if (adminRole == null || organizerRole == null || internalReviewerRole == null || collabRole == null)
                throw new NotFoundException("Không tìm thấy các role trong hệ thống");
            List<string> systemRoles = new List<string>()
            {
                adminRole.RoleId,
                organizerRole.RoleId,
                internalReviewerRole.RoleId,
                collabRole.RoleId
            };
            var availableUsers = await _unitOfWork.PaperRepository.GetAvailableCoAuthorForInclude(conferenceId, systemRoles);
            availableUsers = availableUsers.Where(u => u.UserId != userId).ToList();
            var result = availableUsers.Select(u => new AvailableCoAuthorResponse()
            {
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                FullName = u.FullName,
                UserId = u.UserId,

            }).ToList();
            return result;
        }

        public async Task<int> UpdatePaper(UpdatePaperRequest request, string userId)
        {
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Bài báo với mã {request.PaperId} không tồn tại");
            }
            var paperAuthor = paper.PaperAuthors;
            var rootAuthorCheck = paperAuthor.FirstOrDefault(pa => pa.UserId == userId && pa.IsRootAuthor == true);
            if (rootAuthorCheck == null)
            {
                throw new BadRequestException($"Bạn không sỡ hữu bài báo nên không thể cập nhật");

            }
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                paper.Title = request.Title;
            }
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                paper.Description = request.Description;
            }
            return await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);

        }
    }
}
