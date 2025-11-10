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
using System.IO.Pipelines;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaperService
    {
        Task<int> SubmitAbstract(CreateAbstractRequest request, string userId);
        Task<int> UpdateAbstract(UpdateAbstractRequest request, string userId);
        Task<int> DecideAbstractPaperStatus(UpdateAbstractPaperStatusRequest request, string userId);
        Task<List<PendingAbstractResponse>> GetListPendingAbstract();

        //Task<FullPaperResponse> SubmitFullPaper (CreateFullPaperRequest request, string userId);
        Task<int> SubmitFullPaper(CreateFullPaperRequest request, string userId);
        Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId);
        //cho head reviewer quyết định cuối cùng
        Task<int> DecideFullPaperFinalStatus(UpdateFullPaperStatusRequest request, string userId);

        //Task<int> (UpdateFullPaperReviewStatusRequest request, string userId);

        //gửi review  cho head reviewer xem
        Task<string> SubmitReviewForFullPaper(CreateFullPaperReviewRequest request, string userId);
        Task<List<FullPaperReviewResponse>> GetFullPaperReviewsByFullPaperId(string fullPaperId);


        Task<int> CreateRevisionPaperSubmission(CreateRevisionPaperSubmissionRequest request, string userId);
        Task<int> UpdateRevisionPaperSubmission(UpdateRevisionPaperRevisionSubmissionRequest request, string userId);
        Task<int> DecideReviseStatus(UpdateRevisionStatusRequest request, string userId);
        Task<int> CreateRevisionSubmissionFeedBack(CreateRevisionPaperSubmissionFeedback request, string userId);
        Task<int> CreateRevisionSubmissionResponse(CreateRevisionPaperSubmissionResponse request, string userId);
        Task<int> CreateRevisionReview(CreateRevisionPaperReviewRequest request, string userId);
        Task<List<RevisionPaperReviewResponse>> ListRevisionPaperReview(ListRevisionPaperReviewRequest request, string userId);
        Task<List<PapersAssignedToReviewerResponse>> GetAllAssignedPapersToAReviewer(string userId, string conferenceId);




        Task<string> CreateCameraReady(CreateCameraReadyRequest request, string userId);
        Task<int> UpdateCameraReady(UpdateCameraReadyRequest request, string userId);

        Task<int> DecideCameraReadyStatus(UpdateCameraReadyStatusRequest request, string userId);
        Task<List<Paper>> GetSubmittedPaper(string userId);
        Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId);



        Task<List<Repositories.Models.PaperPhase>> GetListPaperPhases();
        Task<List<Paper>> GetAssignedPapersByReviewerId(string userId);
        Task<List<CameraReadyDtoDetail>> ListPendingCameraReady();
        Task<List<FullPaperDtoDetail>> ListPendingfullpaper();


        Task<List<PaperDetailResponseDTO>> GetListAllPaper();
        Task<List<UnAssignAbstractResponse>> GetUnassignAbstractList();
        Task<PaperDetailForReviewerResponse> GetPaperDetailForReviewer(string paperId, string userId);

        Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId);
        Task<LeaveWaitListResponse> LeaveWaitList(string userId, string conferenceId);
        Task<AddWaitListResponse> AddWaitList(string userId, string conferenceId);


        

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

            if (paperPhase == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper với id {request.PaperId} trong hệ thống");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn cho hội nghị nghiên cứu {paper.Conference!.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Giai đoạn nộp abstract diễn ra từ {activeCurrentPhase.RegistrationStartDate} đến {activeCurrentPhase.RegistrationEndDate}");
            }

            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không đang trong quá trình gửi abstract");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            //if (paper.PresenterId != userId)
            //{
            //    throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            //}
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            }

            var submitterReviewContracts = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            if (request.CoAuthorId != null && request.CoAuthorId.Count > 0)
            {
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
                        if (reviewerContractFound.IsActive == true)
                        {
                            throw new BadRequestException($"Co author với id {coauthorId} hiện đang có hợp đồng reviewer");
                        }
                    }
                    if (isCoauthorReviewerInPaperReviewer == true)
                    {
                        throw new BadRequestException($"Người dùng {coauthorId} đang là reviewer của bài báo này, không thể thêm làm co-author.");
                    }
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
                CreatedAt = ExtensionHelper.GetVietnamTime(),
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


            int finalResult;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result2 = await _unitOfWork.AbstractRepository.CreateAbstractAsync(abstractObj);
                var result1 = await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
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

            if (abstractPaperPhase == null || pendingGlobalStatus == null || rejectedGlobalStatus == null || acceptedGlobalStatus == null || fullPaperPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var basePaper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (basePaper == null)
            {
                throw new NotFoundException($"Không tìm thấy paper với id {request.PaperId} trong hệ thống");
            }
            var activeCurrentPhase = basePaper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {basePaper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Đã quá hạn quyết định abstract");
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
                        abstractPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        basePaper.PaperPhaseId = fullPaperPhase.PaperPhaseId;
                        break;
                    case GlobalStatusEnum.Rejected:
                        abstractPaper.GlobalStatusId = rejectedGlobalStatus.GlobalStatusId;
                        abstractPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
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
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không thể tìm thấy paper id: {request.PaperId} cho user {userId} hiện tại");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai đoạn nộp full paper diễn ra từ {activeCurrentPhase.FullPaperStartDate} đến {activeCurrentPhase.FullPaperEndDate}");
            }
            if (paper.PaperPhaseId != currentFullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không thể gửi full paper vì paper đang không trong trạng thái full paper");
            }
            if (paper.FullPaperId != null)
            {
                throw new BadRequestException($"Full paper file đã có trong hệ thống");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            //if (paper.PresenterId != userId)
            //{
            //    throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            //}
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
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
                CreatedAt = ExtensionHelper.GetVietnamTime(),
                ReviewAt = null,
                Description = request.Description,
                Title = request.Title,
            };
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                result = result + await _unitOfWork.FullPaperRepository.CreateFullPaperAsync(fullPaper);
                paper.FullPaperId = fullPaper.FullPaperId;
                result = result + await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
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

            if (pendingReviewStatus == null || rejectedReviewStatus == null || acceptedReviewStatus == null || reviseStatus == null || currentFullPaperPhase == null || cameraReadyPhase == null || revisePhase == null || pendingGlobal == null)
            {
                throw new NotFoundException($"Không thể tìm thấy các trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Không tìm thấy paper với id {request.PaperId}.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviewStartDate || dateNow > activeCurrentPhase.ReviewEndDate)
            {
                throw new BadRequestException($"Giai đoạn review cho bài báo này diễn ra từ {activeCurrentPhase.ReviewStartDate} đến {activeCurrentPhase.ReviewEndDate}");
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
                        fullPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        paper.PaperPhaseId = cameraReadyPhase.PaperPhaseId;

                        break;
                    case ReviewStatusEnum.Rejected:


                        fullPaper.ReviewStatusId = rejectedReviewStatus.ReviewStatusId;
                        fullPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        break;
                    case ReviewStatusEnum.Revise:



                        fullPaper.ReviewStatusId = reviseStatus.ReviewStatusId;
                        fullPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        paper.PaperPhaseId = revisePhase.PaperPhaseId;


                        break;
                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }
                result += await _unitOfWork.FullPaperRepository.UpdateFullPaperAsync(fullPaper);
                result += await _unitOfWork.PaperRepository.UpdatePaperAsync(paper);
                await _unitOfWork.CommitAsync();

                //await _unitOfWork.CommitAsync();
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
                throw new NotFoundException($"Không thể tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm thấy trong hệ thống");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phải trong trạng thái revise để thực hiện gửi file");
            }


            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            //if (paper.PresenterId != userId)
            //{
            //    throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            //}
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            }

            //var dateNow = ExtensionHelper.GetVietnamDate();
            string revisionDeadlineId = string.Empty;
            var researchConferencePhasesFound = paper.Conference.ResearchConferencePhases;
            foreach (var phase in researchConferencePhasesFound)
            {
                if (phase.ReviseStartDate != null && phase.ReviseEndDate != null && dateNow >= phase.ReviseStartDate && dateNow <= phase.ReviseEndDate)
                {
                    foreach (var deadline in phase.RevisionRoundDeadlines)
                    {
                        if (deadline.EndSubmissionDate != null && deadline.StartSubmissionDate != null && deadline.StartSubmissionDate <= dateNow && dateNow <= deadline.EndSubmissionDate)
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
                        GlobalStatusId = pendingGlobalStatus.GlobalStatusId,
                        CreatedAt = ExtensionHelper.GetVietnamTime(),
                        ReviewAt = null,
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
                    
                    if (!string.IsNullOrEmpty(revisionDeadlineId))
                    {
                        var revisionPaperSubmissionFound = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByRevisionPaperIdAndDeadlineId(paper.RevisionPaperId, revisionDeadlineId);
                        if (revisionPaperSubmissionFound != null)
                        {
                            throw new BadRequestException($"Bạn đã nộp revision, deadline diễn ra từ {revisionPaperSubmissionFound.RevisionDeadlineRound?.StartSubmissionDate} đến {revisionPaperSubmissionFound.RevisionDeadlineRound?.EndSubmissionDate} này ");
                        }
                    }
                }
                revisionPaper.RevisionRound = revisionPaper.RevisionRound + 1;
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
                    Title = request.Title,
                    Description = request.Description,
                    
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
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            var revisionPaperSubmission = await _unitOfWork.RevisionPaperSubmissionRepository.GetRevisionPaperSubmissionByIdAsync(request.RevisionPaperSubmissionId);
            if (revisionPaperSubmission == null)
            {
                throw new NotFoundException($"Không tìm thấy revision paper submission id {request.RevisionPaperSubmissionId} trong hệ thống");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
            {
                throw new NotFoundException($"Không tìm thấy user với id {userId} trong hệ thống assign cho bài báo {request.PaperId}");
            }
            if (paperReviewer.IsHeadReviewer == false)
            {
                throw new NotFoundException($"Chức năng này chỉ dành cho head reviewer.");

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
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            //if (paper.PresenterId != userId)
            //{
            //    throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            //}
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
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
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            var currentRevisePhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());

            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Không thể gửi review vì paper đang không trong trạng thái revise");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
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
            if (currentRevisePhase == null || pendingGlobalStatus == null || acceptedGlobalStatus == null || cameraReadyPaperPhase == null || rejectGlobalStautus == null)
            {
                throw new NotFoundException("Không tìm thấy các trạng thái trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy  paper {request.PaperId} trong hệ thống");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper đang không ở trong trạng thái revise");
            }
            //dùng hàm get
            var revisionPaper = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(request.RevisionPaperId);
            if (revisionPaper == null)
            {
                throw new NotFoundException($"Không tìm thấy  revision paper {request.RevisionPaperId} trong hệ thống");
            }
            if (paper.RevisionPaperId != request.RevisionPaperId)
            {
                throw new NotFoundException($"Paper {request.PaperId} không thuộc revision paper {request.RevisionPaperId}");
            }
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
            if (paperReviewer == null)
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
                        //update instance get:
                        revisionPaper.GlobalStatusId = acceptedGlobalStatus.GlobalStatusId;
                        revisionPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        paper.PaperPhaseId = cameraReadyPaperPhase.PaperPhaseId;
                        break;

                    case GlobalStatusEnum.Rejected:
                        revisionPaper.GlobalStatusId = rejectGlobalStautus.GlobalStatusId;
                        revisionPaper.ReviewAt = ExtensionHelper.GetVietnamTime();
                        break;

                    default:
                        throw new BadRequestException("Trạng thái không khả dụng");
                }
                //call hàm update
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

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, request.PaperId);
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
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.CameraReadyStartDate || dateNow > activeCurrentPhase.CameraReadyEndDate)
            {
                throw new BadRequestException($"Giai đoạn nộp camera ready  diễn ra từ {activeCurrentPhase.CameraReadyStartDate} đến {activeCurrentPhase.CameraReadyEndDate}");
            }

            // Check if paper already has a camera ready
            if (!string.IsNullOrEmpty(paper.CameraReadyId))
            {
                throw new BadRequestException($"Paper with ID {request.PaperId} already has a camera ready record.");
            }

            // Validate that the user is the presenter of the paper
            //if (paper.PresenterId != userId)
            //{
            //    throw new BadRequestException("You are not authorized to create camera ready for this paper.");
            //}

            // Validation: Paper must have either:
            // 1. RevisionPaper with GlobalStatus = "Accepted", OR
            // 2. FullPaper with ReviewStatus = "Accepted"
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
                CreatedAt = ExtensionHelper.GetVietnamTime(),
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

            // Validate that the user is a head reviewer of the paper
            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
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

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
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
                CreatedAt = ExtensionHelper.GetVietnamTime(),
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

            var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAndUserIdAsync(userId, paper.PaperId);
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
            cameraReady.ReviewAt = ExtensionHelper.GetVietnamTime();
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

        public async Task<List<Paper>> GetSubmittedPaper(string userId)
        {
            // Use the new repository method to get papers by user ID in a single query
            var submittedPapers = await _unitOfWork.PaperAuthorRepository.GetPapersByUserIdAsync(userId);

            return submittedPapers;
        }

        public async Task<PaperDetailResponseDtoDetail> getPaperDetail(string paperId)
        {
            // Step 1: Fetch the main Paper entity. This is our starting point.
            // We get Phase and CameraReady here because they are included in the repo method.
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdWithPhaseAsync(paperId);

            if (paper == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy paper với id {paperId}");
            }

            var researchConferencePhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(paper.ConferenceId);
            var roundDeadline = await _unitOfWork.ResearchConferencePhaseRepository.GetRevisionRoundDeadlinesByPhaseIdAsync(researchConferencePhase.ResearchConferencePhaseId);

            //get all authors
            var allAuthor = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
            //get rootauthor
            var paperRootAuthor=  allAuthor.FirstOrDefault(pa => pa.IsRootAuthor ==true);
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
                RootAuthor =  RootAuthor!= null ? new Author {UserId =RootAuthor.UserId,FullName = RootAuthor.FullName } : null,
                CoAuthors = coAuthors?.Select(user =>new Author
                {
                    UserId = user.UserId,
                    FullName = user.FullName
                }).ToList(),
                ResearchPhase = researchConferencePhase !=null ? new ResearchPhaseDtoDetail
                {
                    ResearchConferencePhaseId = researchConferencePhase.ResearchConferencePhaseId,
                    RegistrationStartDate = researchConferencePhase.RegistrationStartDate,
                    RegistrationEndDate = researchConferencePhase.RegistrationEndDate,
                    FullPaperStartDate = researchConferencePhase.FullPaperStartDate,
                    FullPaperEndDate = researchConferencePhase.FullPaperEndDate,
                    ReviewStartDate = researchConferencePhase.ReviewStartDate,
                    ReviewEndDate = researchConferencePhase.ReviewEndDate,
                    ReviseStartDate = researchConferencePhase.ReviseStartDate,
                    ReviseEndDate = researchConferencePhase.ReviseEndDate,
                    CameraReadyStartDate = researchConferencePhase.CameraReadyStartDate,
                    CameraReadyEndDate = researchConferencePhase.ReviewEndDate,
                    ConferenceId = researchConferencePhase.ConferenceId
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
                    Updated= fullPaperEntity.ReviewAt
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


        public async Task<List<Paper>> GetAssignedPapersByReviewerId(string userId)
        {
            //var paperReviewer = await _unitOfWork.PaperReviewerRepository.GetAllPaperReviewersAsync();
            //List<string?> paperIds = paperReviewer.Where( p => p.UserId == userId).Select(s => s.PaperId).ToList();
            //var AssignedPapers = new List<Paper>();
            //foreach (string p in paperIds)
            //{
            //    if (p != null) AssignedPapers.Add(await _unitOfWork.PaperRepository.GetPaperByIdAsync(p));
            //}
            var AssignedPapers = await _unitOfWork.PaperReviewerRepository.getAllAssignedPapers(userId);
            return AssignedPapers;
        }

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
                throw new BadRequestException("Bạn không có quyền hạn để xem paper này");

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
                throw new BadRequestException($"Hội nghị với id {conferenceId} không tồn tại trong hệ thống");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound == null)
            {
                throw new BadRequestException($"Không tồn hàng đợi để xóa");
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
                throw new BadRequestException($"Hội nghị với id {conferenceId} không tồn tại trong hệ thống");
            }
            var conferencePhases = conference.ResearchConferencePhases;
            var firstPhase = conferencePhases.FirstOrDefault(cp => cp.IsActive == true && cp.IsWaitlist == false);
            var waitListPhase = conferencePhases.FirstOrDefault(cp => cp.IsActive == true && cp.IsWaitlist == true);
            if (firstPhase != null && waitListPhase != null)
            {
                throw new BadRequestException("Hiện tại hội nghị đang ở trong 2 giai đoạn bị trùng nhau. Xin vui lòng liên hệ ban tổ chức");
            }
            if (firstPhase == null)
            {
                throw new BadRequestException("Bạn chỉ có thể vô hàng đợi trong khi ở giai đoạn đầu");
            }
            var waitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferenceId);
            if (waitListFound != null)
            {
                throw new BadRequestException($"Bạn đã ở trong hàng đợi rồi");
            }
            var paperWaitListNotifiedStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());
            if (paperWaitListNotifiedStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái hàng đợi trong hệ thống");
            }
            var waitListObj = new PaperWaitList()
            {
                PaperWaitListId = Guid.NewGuid().ToString(),
                ConferenceId = conferenceId,
                UserId = userId,
                CreatedAt = ExtensionHelper.GetVietnamTime(),
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
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với mã {request.PaperId} trong hệ thống");
            }
            if (paper.AbstractId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chưa có abstract để chỉnh sửa");
            }
            var abstractPaper = await _unitOfWork.AbstractRepository.GetAbstractByIdAsync(paper.AbstractId);
            if (abstractPaper!.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException("Abstract hiện không ở trạng thái 'Pending', nên không thể chỉnh sửa.");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn cho hội nghị nghiên cứu {paper.Conference!.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.RegistrationStartDate || dateNow > activeCurrentPhase.RegistrationEndDate)
            {
                throw new BadRequestException($"Giai đoạn sửa abstract diễn ra từ {activeCurrentPhase.RegistrationStartDate} đến {activeCurrentPhase.RegistrationEndDate} nên bạn không thể chỉnh sửa");
            }

            if (paper.PaperPhaseId != paperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không đang trong quá trình sửa abstract");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            }
            var submitterReviewContracts = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
            List<PaperAuthor> paperAuthorList = new List<PaperAuthor>();
            if (request.CoAuthorId != null && request.CoAuthorId.Count > 0 && submitterReviewContracts.Count() >0)
            {
                foreach (var coauthorId in request.CoAuthorId)
                {
                    if (coauthorId == userId)
                    {
                        throw new BadRequestException("Bạn không thể thêm chính mình làm co-author.");
                    }
                    //check coauthor có là reviewer cho bài báo này
                    bool isCoauthorReviewerInPaperReviewer = submitterReviewContracts
                        .Any(pr => pr.UserId == coauthorId);
                    if (isCoauthorReviewerInPaperReviewer == true)
                    {
                        throw new BadRequestException($"Người dùng {coauthorId} đang là reviewer của bài báo này, không thể thêm làm co-author.");
                    }

                    //check coauthor có là external reviewer có contract vs hội nghị 
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


            
          

            int finalResult;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result1 = await _unitOfWork.AbstractRepository.UpdateAbstractAsync(abstractPaper);
                var result2 = 0;
                if (oldPaperCoAuthors.Count()>0 && request.CoAuthorId!=null && request.CoAuthorId.Count() > 0)
                {
                    result2 = await _unitOfWork.PaperAuthorRepository.DeleteMutiplePaperAuthorAsync(oldPaperCoAuthors);
                }
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

        public async Task<int> UpdateFullPaper(UpdateFullPaperRequest request, string userId)
        {
            var pendingFullPaperReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription());

            if (fullPaperPhase == null || pendingFullPaperReviewStatus == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với mã {request.PaperId} trong hệ thống");
            }
            if (paper.FullPaperId == null)
            {
                throw new NotFoundException($"Bài báo {paper.PaperId} chưa có fullpaper để chỉnh sửa");
            }
            var fullPaper = await _unitOfWork.FullPaperRepository.GetFullPaperByIdAsync(paper.FullPaperId);
            if (fullPaper!.ReviewStatusId != pendingFullPaperReviewStatus.ReviewStatusId)
            {
                throw new BadRequestException($"Full paper hiện không ở trạng thái 'Pending', nên không thể chỉnh sửa. Trạng thái hiện tại là {fullPaper.ReviewStatus?.Name}");
            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn cho hội nghị nghiên cứu {paper.Conference!.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.FullPaperStartDate || dateNow > activeCurrentPhase.FullPaperEndDate)
            {
                throw new BadRequestException($"Giai đoạn sửa full paper diễn ra từ {activeCurrentPhase.FullPaperStartDate} đến {activeCurrentPhase.FullPaperEndDate} nên bạn không thể chỉnh sửa");
            }

            if (paper.PaperPhaseId != fullPaperPhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper hiện tại không đang trong quá trình sửa full paper");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
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
                throw new NotFoundException($"Không thể tìm thấy trạng thái tương ứng trong hệ thống");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper id {request.PaperId} không tìm thấy trong hệ thống");
            }
            if (paper.RevisionPaperId == null)
            {
                throw new NotFoundException($"Mã bài báo {request.PaperId} không tìm thấy revision id trong hệ thống");

            }
            var activeCurrentPhase = paper.ResearchConferencePhase;
            if (activeCurrentPhase == null)
            {
                throw new NotFoundException($"Không tìm thấy  giai đoạn nào đang diễn ra cho hội nghị nghiên cứu {paper.Conference.ConferenceName}");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (dateNow < activeCurrentPhase.ReviseStartDate || dateNow > activeCurrentPhase.ReviseEndDate)
            {
                throw new BadRequestException($"Giai đoạn revise diễn ra từ {activeCurrentPhase.ReviseStartDate} đến {activeCurrentPhase.ReviseEndDate}");
            }
            if (paper.PaperPhaseId != currentRevisePhase.PaperPhaseId)
            {
                throw new BadRequestException($"Paper phải trong trạng thái revise để thực hiện update");
            }
            var rootAuthorCheck = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == userId);
            if (rootAuthorCheck == null)
            {
                throw new NotFoundException($"Bạn không có quyền sỡ hữu bài báo này");
            }
            var revisionPaperFound = await _unitOfWork.RevisionPaperRepository.GetRevisionPaperByIdAsync(paper.RevisionPaperId);
            if (revisionPaperFound == null) 
            {
                throw new NotFoundException($"Không tìm thấy revision paper với id {paper.RevisionPaperId}");
            }
            if (revisionPaperFound.GlobalStatusId != pendingGlobalStatus.GlobalStatusId)
            {
                throw new BadRequestException($"Revision paper phải trong trạng thái pending để thực hiện update");
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
            if (dateNow < currentRevisionPaperSubmissionDeadline!.StartSubmissionDate || dateNow> currentRevisionPaperSubmissionDeadline!.EndSubmissionDate)
            {
                throw new BadRequestException($"Bạn không thể chỉnh sửa vì deadline revision submission này từ {currentRevisionPaperSubmissionDeadline.StartSubmissionDate} đến {currentRevisionPaperSubmissionDeadline.EndSubmissionDate}");
            }
            var revisionSubmissionFeedbackList = currentRevisionPaperSubmission.RevisionSubmissionFeedbacks;
            if (revisionSubmissionFeedbackList.Any())
            {
                throw new BadRequestException($"Bạn không thể chỉnh sửa vì  revision submission này vì hiện tại đã có head reviewer đưa ra đánh giá. ");
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
                result =  await _unitOfWork.RevisionPaperSubmissionRepository.UpdateRevisionPaperSubmissionAsync(currentRevisionPaperSubmission);
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
