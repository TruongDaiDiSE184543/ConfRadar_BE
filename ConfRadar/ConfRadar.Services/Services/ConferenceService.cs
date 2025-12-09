using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.DTOs.General;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferenceService
    {

        //Task<List<ConferenceResponse>> GetAllConferencesAsync();
        Task<PagedResult<ConferenceResponseDTO>> GetAllConferencesPaginatedAsync(int page, int pageSize);

        // NEW ENDPOINTS
        // Endpoint 1: Get all conferences with their price phases (with pagination/filtering)
        Task<PagedResult<ConferenceWithPricesResponse>> GetConferencesWithPricesAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null, bool? isResearch = null, string? rankingCategoryId = null, bool? allowListener = null, bool? noReviewerFee = null, int? totalRevisionRound = 0, string? targetAudience = null);

        // Endpoint 2: Get detailed technical conference data
        Task<TechnicalConferenceDetailResponse> GetTechnicalConferenceDetailAsync(string conferenceId, string? userId);

        // Endpoint 3: Get conferences by status ID with filtering
        Task<PagedResult<ConferenceResponseDTO>> GetConferencesByStatusAsync(string conferenceStatusId, int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // Endpoint 4: Get conferences with step completion status
        Task<PagedResult<ConferenceStepCompletionStatusResponse>> GetTechnicalConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // NEW ENDPOINT 5: Get all pending conferences
        Task<PagedResult<ConferenceResponseDTO>> GetPendingConferencesAsync(int page, int pageSize, string? searchKeyword = null);

        // NEW ENDPOINT 6: Approve conference (change status from pending to preparing)
        Task<bool> ApproveConferenceAsync(string conferenceId, ApproveConferenceRequest request);


        // NEW ENDPOINT 7: Get detailed research conference data
        Task<DTOs.Conference.ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId, string? userId);

        // NEW ENDPOINT 8: Get research conference step completion status
        Task<PagedResult<DTOs.Conference.ResearchConferenceStepCompletionStatusResponse>> GetResearchConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // NEW ENDPOINT 9: Check if technical conference has completed a specific step
        Task<bool> CheckTechnicalConferenceStepCompletionAsync(string conferenceId, string step);

        // NEW ENDPOINT 10: Check if research conference has completed a specific step
        Task<bool> CheckResearchConferenceStepCompletionAsync(string conferenceId, string step);

        // NEW ENDPOINT 11: Get list of research conferences with pagination and filtering
        Task<PagedResult<DTOs.Conference.ResearchConferenceDetailResponse>> GetResearchConferencesListAsync(int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null, string? userId = null, bool isOrganizer = false);

        // NEW ENDPOINT 12: Get list of technical conferences with pagination and filtering
        Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorAsync(
          int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
          string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
          string? userId = null, bool isOrganizer = false, string collboratorId = null, string organization = null, bool excludeDraft = false);

        Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByOrganizerAsync(
         int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
         string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
         string? userId = null, bool isOrganizer = false);


        // NEW ENDPOINT 13: Get detailed research conference data for organizer
        Task<DTOs.Conference.ResearchConferenceDetailResponse> GetDetailResearchForOrganizerAsync(string conferenceId);

        // NEW ENDPOINT 14: Get detailed technical conference data for organizer and collaborator
        Task<DTOs.Conference.TechnicalConferenceDetailResponse> GetDetailTechnicalAsync(string conferenceId, string? userId, bool isOrganizer = false);

        // ENdPOINT 15: Update conference status log the transition in conference timeline
        Task<bool> ChangeConferenceStatus(string userId, string conferenceId, string newStatus, string? reason = null);
        Task<List<ConferenceWithStatusNameResponse>> GetAllConferenceWithStatusByUserId(string userId, string statusId);


        Task<int> SubmitConferenceFeedback(CreateConferenceFeedbackRequest request, string userId);
        Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId);
        Task<List<ConferenceResponseDTO>> GetConferenceByAssignedPapers(string? userId);
        Task<bool> RequestOrganizerApproval(string confId, string userId);
        Task<bool> ActivateNextPhase(string confId, string userId);
        Task<List<SkeletonTechConfResponse>> getSkeletonTechConf(string collaboratorId);
        Task<bool> AutoAdjustTimelineForOnHoldAsync(string conf, string userId);
        Task<PagedResult<TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorOnlyDraftAsync(int page, int pageSize, string? searchKeyword, string? cityId, DateOnly? startDate, DateOnly? endDate, string? userId, bool isOrganizer, string? collaboratorId, string? organizationName);
        Task<PagedResult<TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorNoDraftAsync(int page, int pageSize, string? conferenceStatusId, string? searchKeyword, string? cityId, DateOnly? startDate, DateOnly? endDate, string? userId, bool isOrganizer, string? collaboratorId, string? organizationName);
        Task<bool> DisableContractedConference(string confId, string? reason = null);
        Task<bool> ToReadyFromDisabledContractedConference(string conferenceId, string? reason);
    }

    public class ConferenceService : IConferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConferenceStatusService _conferenceStatusService;
        private readonly IConferenceTimelineService _conferenceTimelineService;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly ISystemConfigurationService _systemConfigurationService;
        private readonly ITimeProviderService _timeProviderService;
        private readonly INotificationService _notificationService;

        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceService(IUnitOfWork unitOfWork, IConferenceStatusService conferenceStatusService, IConferenceTimelineService conferenceTimelineService,
            IObjectStorageFileService objectStorageFileService, ITokenService tokenService,
            ISystemConfigurationService systemConfigurationService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings, ITimeProviderService timeProviderService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _conferenceStatusService = conferenceStatusService;
            _conferenceTimelineService = conferenceTimelineService;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _systemConfigurationService = systemConfigurationService;
            _timeProviderService = timeProviderService;
            _objectStorageSettings = objectStorageSettings.Value;
            _notificationService = notificationService;
        }





        ///// <summary>
        ///// Adds the base MinIO URL to a file URL if it's not already a full URL
        ///// </summary>
        //private string? AddBaseUrlToUrl(string? url)
        //{
        //    if (string.IsNullOrEmpty(url))
        //        return url;

        //    // If the URL already starts with http/https, return as is
        //    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        //        return url;

        //    // Prepend the base URL from configuration
        //    return _objectStorageSettings.EndPoint?.TrimEnd('/') + "/" + url.TrimStart('/');
        //}

        #region Helper methods to validateDate

        private async Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new BadRequestException($"Không tìm thấy conf id {conferenceId} này");
            }

            // Get current status name from the conference status ID
            var currentStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conference.ConferenceStatusId);
            if (currentStatus == null)
            {
                throw new BadRequestException("Không tìm thấy trạnng thái hiện tại của hội nghị");
            }
            var onholdStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.OnHold.GetDescription());
            var cancelledStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Cancelled.GetDescription());

            // Get the new status by name
            var newStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(newStatusName);
            if (newStatus == null)
            {
                throw new BadRequestException($"Không tìm thấy trạng thái {newStatus}");
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate the status transition
                bool isValidTransition = await _conferenceStatusService.IsStatusTransitionValidAsync(currentStatus.ConferenceStatusName, newStatus.ConferenceStatusName);
                if (!isValidTransition)
                {
                    throw new BadRequestException($"Chuyển trạng thái từ '{currentStatus.ConferenceStatusName}' sang '{newStatusName}' không hợp lệ");
                }

                //from onhold to ready
                if (newStatus.ConferenceStatusName == "Ready")
                {
                    if (currentStatus.ConferenceStatusName == ConferenceStatusEnum.Disabled.GetDescription())
                    {
                        await ProcessingFromDisableToReady(conference, newStatus.ConferenceStatusId, currentStatus.ConferenceStatusId);
                    }
                    else if (currentStatus.ConferenceStatusName == "OnHold")
                    {
                        await OnholdToReadyValidAsync(conference, newStatus.ConferenceStatusId, currentStatus.ConferenceStatusId);
                    }
                    else await ValidateForReadyStateAsync(conference);
                }

                if (newStatus.ConferenceStatusName == "Cancelled")
                    await ValidateForCancelledStateAsync(conference);


                if (newStatus.ConferenceStatusName == ConferenceStatusEnum.Completed.GetDescription())
                    await ValidateForComplete(conference);



                // Update the conference status
                conference.ConferenceStatusId = newStatus.ConferenceStatusId;

                // Create a timeline record for the status change
                var timelineRecord = new CreateConferenceTimelineRequest
                {
                    ConferenceId = conferenceId,
                    ChangeDate = await _timeProviderService.GetVietnamDate(),
                    PreviousStatusId = currentStatus.ConferenceStatusId,
                    AfterwardStatusId = newStatus.ConferenceStatusId,
                    Reason = reason
                };

                await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);

                // Insert the timeline record after the status change is saved
                await _conferenceTimelineService.CreateConferenceTimelineAsync(timelineRecord.ToModel());

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
                return false;
            }

        }


        private async Task<List<string>> ValidateConferenceTimelineAsync(Conference conf, Func<DateOnly?, bool> dateOnlyValidationRule, CollaboratorContract? contract = null)
        {
            var invalidMessages = new List<string>();

            // Hàm helper nội bộ để giảm lặp code và xử lý null an toàn
            void AddIfInvalid(DateOnly? date, string name)
            {
                if (date.HasValue && dateOnlyValidationRule(date))
                {
                    // Chỉ thêm vào message nếu thực sự có lỗi và có ngày để hiển thị
                    invalidMessages.Add($"{name} ({date.Value:dd/MM/yyyy})");
                }
            }

            // --- tìm contract cho hội nghị nếu đầu vào là null ---
            if (conf.IsInternalHosted == false && contract == null)
            {
                contract = await _unitOfWork.CollaboratorContractRepository.GetCollaboratorContractByConferenceId(conf.ConferenceId);
            }

            // hàm để check có nên kiểm tra bước này không
            bool ShouldCheck(Func<CollaboratorContract, bool?> contractProperty)
            {
                if (conf.IsInternalHosted == true) return true;
                if (contract == null) return false;
                return contractProperty(contract) == true;
            }


            // 1. Kiểm tra Conference
            AddIfInvalid(conf.StartDate, "Ngày bắt đầu hội nghị");
            AddIfInvalid(conf.EndDate, "Ngày kết thúc hội nghị");
            AddIfInvalid(conf.TicketSaleEnd, "Ngày kết thúc bán vé");

            // 2. Kiểm tra PricePhase và RefundPolicy (Conditional)
            if (ShouldCheck(c => c.IsPriceStep))
            {
                var allPricesWithPhasesAndPolicies = await _unitOfWork.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(conf.ConferenceId);
                foreach (var price in allPricesWithPhasesAndPolicies)
                {
                    foreach (var phase in price.PricePhases)
                    {
                        AddIfInvalid(phase.EndDate, $"Giai đoạn bán vé '{phase.PhaseName}'");
                        foreach (var policy in phase.RefundPolicies)
                        {
                            AddIfInvalid(policy.RefundDeadline, $"Hạn chót hoàn tiền của phase '{phase.PhaseName}'");
                        }
                    }
                }
            }

            // 3. Kiểm tra ConferenceSession (Conditional)
            if (ShouldCheck(c => c.IsSessionStep))
            {
                var allSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conf.ConferenceId);
                foreach (var session in allSessions)
                {
                    AddIfInvalid(session.SessionDate, $"Phiên '{session.Title}'");
                }
            }


            // 4. Kiểm tra Research Conference (nếu có)
            if (conf.IsResearchConference == true)
            {
                var allResearchPhases = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conf.ConferenceId);
                foreach (var phase in allResearchPhases)
                {

                    // SỬA LỖI LOGIC HIỂN THỊ NGÀY Ở ĐÂY
                    AddIfInvalid(phase.RegistrationEndDate, $"{phase.PhaseOrder}: Hạn chót đăng ký");
                    AddIfInvalid(phase.FullPaperEndDate, $"{phase.PhaseOrder}: Hạn chót nộp Full Paper");
                    AddIfInvalid(phase.ReviewEndDate, $"{phase.PhaseOrder}: Hạn chót phản biện");
                    AddIfInvalid(phase.ReviseEndDate, $"{phase.PhaseOrder}: Hạn chót sửa đổi");
                    AddIfInvalid(phase.CameraReadyEndDate, $"{phase.PhaseOrder}: Hạn chót Camera Ready");
                    foreach (RevisionRoundDeadline revisionRoundDeadline in phase.RevisionRoundDeadlines)
                    {
                        AddIfInvalid(revisionRoundDeadline.EndSubmissionDate, $"{revisionRoundDeadline.RoundNumber}: trong qua khứ");
                    }

                    // (Bạn có thể thêm kiểm tra cho RevisionRoundDeadline ở đây nếu cần)
                }
            }

            return invalidMessages;
        }

        private bool IsDateInvalidatedByOnHold(DateOnly onHoldStartDate, DateOnly today, DateOnly? dateToCheck)
        {
            // Nếu không có ngày để kiểm tra, nó không thể bị lỗi thời.
            if (!dateToCheck.HasValue)
            {
                return false;
            }

            // Điều kiện "lỗi thời":
            // 1. Mốc thời gian đó (dateToCheck) đáng lẽ phải xảy ra SAU KHI hoặc VÀO NGÀY bị OnHold.
            // 2. VÀ mốc thời gian đó bây giờ đã nằm TRONG QUÁ KHỨ.
            return dateToCheck.Value >= onHoldStartDate && dateToCheck.Value < today;
        }


        private async Task ValidateForCancelledStateAsync(Conference conference)
        {
            //get not refunded ticket
            var refundedTicket = await _unitOfWork.TicketRepository.GetNotRefundedTicketsByConferenceIdAsync(conference.ConferenceId);
            var invalidMessages = new List<string>();
            if (refundedTicket.Any())
            {
                foreach (var ticket in refundedTicket)
                {
                    string typeOfTicket = ticket.PricePhase.ConferencePrice.IsAuthor.Value ? "tác giả" : "thường";
                    invalidMessages.Add($"Còn vé {ticket.TicketId} thuộc loại {typeOfTicket} của khách với ID {ticket.UserId} chưa được refund");
                }
            }

            if (conference.IsResearchConference == true)
            {
                //get not rejected paper
                var reviewStatusNotRejected = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());
                var globalStatusRejected = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                var notRejectedPapers = await _unitOfWork.PaperRepository.GetAllNotRejectEdPaper(globalStatusRejected, reviewStatusNotRejected, conference.ConferenceId);

                foreach (var paper in notRejectedPapers)
                {
                    invalidMessages.Add($"Còn paper với ID {paper.PaperId} ở phase {paper.PaperPhase.PhaseName} chưa trong trạng thái rejected");
                }
            }


            if (invalidMessages.Any())
            {
                string response = "Không thể chuyển sang trạng thái cancelled vì : " + string.Join("\n- ", invalidMessages.Distinct());
                throw new Exception(response);
            }
        }



        private async Task OnholdToReadyValidAsync(Conference conf, string readyId, string onHoldId)
        {
            var onHoldStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.OnHold.GetDescription());
            var onHoldTimelineEntry = await _unitOfWork.ConferenceTimelineRepository.GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(conf.ConferenceId, readyId, onHoldId);
            if (onHoldTimelineEntry == null)
                throw new BadRequestException("Không tìm thấy lịch sử chuyển sang trạng thái 'OnHold'.");



            var onHoldStartDate = onHoldTimelineEntry.ChangeDate;
            var today = await _timeProviderService.GetVietnamDate();
            var todayAsDateTime = await _timeProviderService.GetVietnamTime();

            // --- ĐỊNH NGHĨA "RULE" ---
            Func<DateOnly?, bool> dateOnlyRule = (DateOnly? dateToCheck) =>
                dateToCheck.HasValue && dateToCheck.Value >= onHoldStartDate && dateToCheck.Value < today;

            //Func<DateTime?, bool> dateTimeRule = (DateTime? dateTimeToCheck) =>
            //{
            //    if (!dateTimeToCheck.HasValue) return false;
            //    var dateOnly = DateOnly.FromDateTime(dateTimeToCheck.Value);
            //    return dateOnly >= onHoldStartDate && dateOnly < today;
            //};

            // --- GỌI "ENGINE" VỚI RULE TRÊN ---
            var invalidMessages = await ValidateConferenceTimelineAsync(conf, dateOnlyRule);

            if (invalidMessages.Any())
            {
                string errorMessage = "Không thể chuyển về trạng thái 'Ready'. Các mốc thời gian sau đã bị lỗi thời và cần được cập nhật:\n- "
                                    + string.Join("|", invalidMessages.Distinct());
                throw new BadRequestException(errorMessage);
            }
        }


        private async Task ValidateForReadyStateAsync(Conference conf)
        {
            var invalidMessages = new List<string>();
            CollaboratorContract? contract = null;

            if (conf.IsInternalHosted == false)
            {
                contract = await _unitOfWork.CollaboratorContractRepository.GetCollaboratorContractByConferenceId(conf.ConferenceId);
                if (contract == null)
                {
                    // A non-internal conference MUST have a contract to be Ready.
                    invalidMessages.Add("Hội nghị liên kết (Collaborator) phải có một hợp đồng hợp lệ.");
                }
            }


            // --- BƯỚC A: KIỂM TRA SỰ ĐẦY ĐỦ THÔNG TIN (CONTRACT-AWARE) ---

            // Function to decide if a check should be performed
            bool ShouldCheck(Func<CollaboratorContract, bool?> contractProperty)
            {
                if (conf.IsInternalHosted == true) return true; // Internal always checks
                if (contract == null) return false; // No contract, can't check
                return contractProperty(contract) == true; // Check only if contract flag is true
            }

            // Validate Prices only if the step is enabled
            if (ShouldCheck(c => c.IsPriceStep))
            {
                var price = await _unitOfWork.ConferencePriceRepository.AnyConferencePriceWithAtLeastOnePricePhase(conf.ConferenceId);
                if (price == null)
                    invalidMessages.Add("Hội nghị phải có ít nhất một loại vé, trong đó có ít nhất một phase bán vé.");
            }

            // Validate Sessions only if the step is enabled
            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conf.ConferenceId);
            if (ShouldCheck(c => c.IsSessionStep))
            {
                if (!sessions.Any())
                    invalidMessages.Add("Hội nghị phải có ít nhất một phiên (session).");
            }

            // Validate Policies only if the step is enabled
            if (ShouldCheck(c => c.IsPolicyStep))
            {
                var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conf.ConferenceId);
                if (!policies.Any())
                    invalidMessages.Add("Hội nghị phải có ít nhất một chính sách.");
            }

            // Validate Sponsors only if the step is enabled
            if (ShouldCheck(c => c.IsSponsorStep))
            {
                var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conf.ConferenceId);
                if (!sponsors.Any())
                    invalidMessages.Add("Hội nghị phải có ít nhất một nhà tài trợ.");
            }


            // Kiểm tra nếu là hội nghị kỹ thuật, phiên phải có ít nhất một diễn giả
            if (conf.IsResearchConference == false)
            {
                var technicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conf.ConferenceId);
                if (technicalDetail == null)
                {
                    invalidMessages.Add("Hội nghị kỹ thuật phải có thông tin chi tiết kỹ thuật.");
                }

                // Kiểm tra các phiên trong hội nghị kỹ thuật có ít nhất một diễn giả
                //foreach (var session in sessions)
                //{
                //    var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(session.ConferenceSessionId);
                //    if (!speakers.Any())
                //    {
                //        invalidMessages.Add($"Phiên '{session.Title}' trong hội nghị kỹ thuật phải có ít nhất một diễn giả.");
                //    }
                //}
            }
            // Kiểm tra nếu là hội nghị nghiên cứu
            else
            {
                // Kiểm tra các phiên trong hội nghị nghiên cứu có ít nhất một tác giả trình bày
                //foreach (var session in sessions)
                //{
                //    var presentAuthors = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorsBySessionIdAsync(session.ConferenceSessionId);
                //    if (!presentAuthors.Any())
                //    {
                //        invalidMessages.Add($"Phiên '{session.Title}' trong hội nghị nghiên cứu phải có ít nhất một tác giả trình bày.");
                //    }
                //}

                // Kiểm tra hội nghị nghiên cứu có các thông tin bắt buộc
                var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conf.ConferenceId);
                if (researchDetail == null)
                {
                    invalidMessages.Add("Hội nghị nghiên cứu phải có thông tin chi tiết nghiên cứu.");
                }

                // Kiểm tra các thành phần của hội nghị nghiên cứu - mỗi loại phải có ít nhất một
                var researchPhases = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conf.ConferenceId);
                if (!researchPhases.Any())
                {
                    invalidMessages.Add("Hội nghị nghiên cứu phải có ít nhất một giai đoạn nghiên cứu.");
                }

                var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conf.ConferenceId);
                if (!materialDownloads.Any())
                {
                    invalidMessages.Add("Hội nghị nghiên cứu phải có ít nhất một tài liệu tải về.");
                }

                var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conf.ConferenceId);
                if (!rankingFileUrls.Any())
                {
                    invalidMessages.Add("Hội nghị nghiên cứu phải có ít nhất một URL tệp xếp hạng.");
                }

                var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conf.ConferenceId);
                if (!rankingReferenceUrls.Any())
                {
                    invalidMessages.Add("Hội nghị nghiên cứu phải có ít nhất một URL tham khảo xếp hạng.");
                }
            }


            // --- BƯỚC B: KIỂM TRA NGÀY THÁNG LỖI THỜI ---
            var today = await _timeProviderService.GetVietnamDate();
            //var todayAsDateTime = DateTime.Now;

            // --- ĐỊNH NGHĨA "RULE" ---
            Func<DateOnly?, bool> dateOnlyRule = (DateOnly? dateToCheck) =>
                dateToCheck.HasValue && dateToCheck.Value < today;

            //Func<DateTime?, bool> dateTimeRule = (dateTimeToCheck) =>
            //    dateTimeToCheck.HasValue && dateTimeToCheck.Value < todayAsDateTime;

            // --- GỌI "ENGINE" VỚI RULE TRÊN ---
            var timelineErrors = await ValidateConferenceTimelineAsync(conf, dateOnlyRule, contract);
            invalidMessages.AddRange(timelineErrors); // Thêm các lỗi timeline vào danh sách chung

            if (invalidMessages.Any())
            {
                string errorMessage = "Không thể chuyển sang trạng thái 'Ready'. Vui lòng khắc phục các vấn đề sau:\n- "
                                    + string.Join("|", invalidMessages.Distinct());
                throw new BadRequestException(errorMessage);
            }
        }

        private async Task ValidateForComplete(Conference conf)
        {
            // 1. Lấy danh sách session

            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdWithRoomAsync(conf.ConferenceId);

            // 2. Lấy thời gian hiện tại 
            var now = await _timeProviderService.GetVietnamTime();

            // 3. Validate ngày bắt đầu hội nghị (Phòng trường hợp bấm nhầm trước ngày diễn ra)
            // Dùng StartDate của Conference làm chốt chặn đầu tiên
            var confStartDateTime = conf.StartDate.Value.ToDateTime(new TimeOnly(0, 0, 0));
            if (now < confStartDateTime)
                throw new BadRequestException($"Hội nghị chưa diễn ra (Ngày {conf.StartDate:dd/MM/yyyy}). Không thể chuyển sang trạng thái hoàn thành.");

            // 4. Tìm phiên khai mạc (First Session)
            var firstSession = sessions
                .OrderBy(cs => cs.SessionDate)
                .ThenBy(cs => cs.StartTime)
                .FirstOrDefault();

            // 5. Kiểm tra điều kiện "Phiên đầu tiên phải kết thúc"
            if (firstSession != null && firstSession.EndTime.HasValue && now <= firstSession.EndTime.Value)
            {
                throw new BadRequestException(
                    $"Không thể chuyển sang trạng thái 'Completed' vì phiên khai mạc '{firstSession.Title}' chưa kết thúc.\n" +
                    $"- Thời gian kết thúc dự kiến: {firstSession.EndTime:dd/MM/yyyy HH:mm}\n" +
                    $"- Thời gian hiện tại: {now:dd/MM/yyyy HH:mm}\n"
                );
            }
        }

        private async Task ProcessingFromDisableToReady(Conference conference, string readyStatusId, string disabledStatusId)
        {
            var disabledTimelineEntry = await _unitOfWork.ConferenceTimelineRepository
             .GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(conference.ConferenceId, readyStatusId, disabledStatusId);

            if (disabledTimelineEntry == null)
                throw new BadRequestException("Không tìm thấy lịch sử bị Disabled để khôi phục.");

            var disabledDate = disabledTimelineEntry.ChangeDate.Value;
            var today = await _timeProviderService.GetVietnamDate();
            int daysToShift = today.DayNumber - disabledDate.DayNumber;

            // 2. Tự động dịch chuyển thời gian (NẾU cần thiết)
            if (daysToShift > 0)
            {
                await ExecuteTimelineShiftInternal(conference, daysToShift, disabledDate);
            }
        }



        // Hàm nội bộ dùng chung: Chỉ chịu trách nhiệm cộng ngày
        private async Task ExecuteTimelineShiftInternal(Conference conf, int daysToShift, DateOnly pivotDate)
        {
            // Helper func
            DateOnly? Shift(DateOnly? d)
            {
                if (!d.HasValue) return null;
                if (d.Value < pivotDate) return d; // Ngày cũ giữ nguyên
                return d.Value.AddDays(daysToShift); // Ngày mới/tương lai thì cộng thêm
            }

            DateTime? ShiftDt(DateTime? dt)
            {
                if (!dt.HasValue) return null;
                var d = DateOnly.FromDateTime(dt.Value);
                if (d < pivotDate) return dt;
                return dt.Value.AddDays(daysToShift);
            }

            CollaboratorContract? contract = null;
            if (conf.IsInternalHosted == false)
            {
                contract = await _unitOfWork.CollaboratorContractRepository.GetCollaboratorContractByConferenceId(conf.ConferenceId);
            }

            // dựa vào istep trong collab contract để test
            bool ShouldProcess(Func<CollaboratorContract, bool?> contractProperty)
            {
                if (conf.IsInternalHosted == true) return true; // nội bộ luôn check
                if (contract == null) return false; //check cho an toàn
                return contractProperty(contract) == true;
            }


            // A. Cập nhật Conference
            conf.TicketSaleStart = Shift(conf.TicketSaleStart.Value);
            conf.TicketSaleEnd = Shift(conf.TicketSaleEnd.Value);
            conf.StartDate = Shift(conf.StartDate);
            conf.EndDate = Shift(conf.EndDate);
            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conf);

            // B. Cập nhật PricePhases (Conditional)
            if (ShouldProcess(c => c.IsPriceStep))
            {
                var prices = await _unitOfWork.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(conf.ConferenceId);
                foreach (var price in prices)
                {
                    foreach (var phase in price.PricePhases)
                    {
                        phase.StartDate = Shift(phase.StartDate);
                        phase.EndDate = Shift(phase.EndDate);
                        await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(phase);
                        foreach (var policy in phase.RefundPolicies)
                        {
                            policy.RefundDeadline = Shift(policy.RefundDeadline);
                            await _unitOfWork.ConferenceRefundPolicyRepository.UpdateConferenceRefundPolicyAsync(policy);
                        }
                    }
                }
            }

            // C. Cập nhật Sessions (Conditional)
            if (ShouldProcess(c => c.IsSessionStep))
            {
                var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conf.ConferenceId);
                foreach (var session in sessions)
                {
                    session.SessionDate = Shift(session.SessionDate);
                    session.StartTime = ShiftDt(session.StartTime);
                    session.EndTime = ShiftDt(session.EndTime);
                    await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);
                }
            }



            // D. Cập nhật Research Phases 
            if (conf.IsResearchConference == true)
            {
                var researchPhases = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conf.ConferenceId);
                foreach (var phase in researchPhases)
                {
                    phase.RegistrationStartDate = Shift(phase.RegistrationStartDate);
                    phase.RegistrationEndDate = Shift(phase.RegistrationEndDate);

                    phase.AbstractDecideStatusStart = Shift(phase.AbstractDecideStatusStart);
                    phase.AbstractDecideStatusEnd = Shift(phase.AbstractDecideStatusEnd);

                    phase.FullPaperStartDate = Shift(phase.FullPaperStartDate);
                    phase.FullPaperEndDate = Shift(phase.FullPaperEndDate);

                    phase.ReviewStartDate = Shift(phase.ReviewStartDate);
                    phase.ReviewEndDate = Shift(phase.ReviewEndDate);

                    phase.FullPaperDecideStatusStart = Shift(phase.FullPaperDecideStatusStart);
                    phase.FullPaperDecideStatusEnd = Shift(phase.FullPaperDecideStatusEnd);

                    phase.ReviseStartDate = Shift(phase.ReviseStartDate);
                    phase.ReviseEndDate = Shift(phase.ReviseEndDate);

                    phase.RevisionPaperDecideStatusStart = Shift(phase.RevisionPaperDecideStatusStart);
                    phase.RevisionPaperDecideStatusEnd = Shift(phase.RevisionPaperDecideStatusEnd);

                    phase.CameraReadyStartDate = Shift(phase.CameraReadyStartDate);
                    phase.CameraReadyEndDate = Shift(phase.CameraReadyEndDate);

                    phase.CameraReadyDecideStatusStart = Shift(phase.CameraReadyDecideStatusStart);
                    phase.CameraReadyDecideStatusEnd = Shift(phase.CameraReadyDecideStatusEnd);

                    await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(phase);

                    // D.1 Cập nhật RevisionRoundDeadlines
                    var deadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(phase.ResearchConferencePhaseId);
                    foreach (var deadline in deadlines)
                    {
                        deadline.StartSubmissionDate = Shift(deadline.StartSubmissionDate);
                        deadline.EndSubmissionDate = Shift(deadline.EndSubmissionDate);
                        await _unitOfWork.RevisionRoundDeadlineRepository.UpdateCsAsync(deadline);
                    }
                }
            }
        }

        #endregion


        public async Task<PagedResult<ConferenceResponseDTO>> GetAllConferencesPaginatedAsync(int page, int pageSize)
        {
            var query = _unitOfWork.ConferenceRepository.GetAllConferences();

            var totalCount = await query.CountAsync();
            var readystatusId = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var pagedConferences = await query
                .Where(c => c.ConferenceStatusId == readystatusId.ConferenceStatusId)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var responses = pagedConferences.Select(conference => new ConferenceResponseDTO
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                //Capacity = conference.Capacity,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                //CreatedAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                //IsActive = conference.IsActive,
                //UserId = conference.UserId,
                //LocationId = conference.LocationId,
                ConferenceCategoryId = conference.ConferenceCategoryId,

            }).ToList();
            return new PagedResult<ConferenceResponseDTO>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // NEW ENDPOINTS IMPLEMENTATION

        public async Task<PagedResult<ConferenceWithPricesResponse>> GetConferencesWithPricesAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null, bool? isResearch = null, string? rankingCategoryId = null, bool? allowListener = null, bool? noReviewerFee = null, int? totalRevisionRound = null, string? targetAudience = null)
        {
            var readyStatus = await _unitOfWork.ConferenceStatusRepository
       .GetConferenceStatusByName(ConferenceStatusEnum.Ready.GetDescription());

            // 1. Gọi Repo (đã thêm Include Technical)
            IQueryable<Conference> query = _unitOfWork.ConferenceRepository
                .GetConferencesWithPrice(readyStatus.ConferenceStatusId);

            // 2. Filter Cơ bản
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var lowerKeyword = searchKeyword.ToLower(); // Tối ưu: lower 1 lần bên ngoài
                query = query.Where(c => c.ConferenceName.ToLower().Contains(lowerKeyword) ||
                                         c.Description.ToLower().Contains(lowerKeyword));
            }

            if (!string.IsNullOrEmpty(cityId))
                query = query.Where(c => c.CityId == cityId);

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate);

            // 3. Filter Loại Hội Nghị (Research vs Technical)
            if (isResearch.HasValue)
            {
                query = query.Where(c => c.IsResearchConference == isResearch.Value);
            }

            // 4. Filter Chi tiết Research (Chạy độc lập, tự động check null)
            if (!string.IsNullOrEmpty(rankingCategoryId))
            {
                // Tự động lọc ra những cái có ResearchDetail và đúng Ranking
                query = query.Where(c => c.ResearchConferenceDetail != null &&
                                         c.ResearchConferenceDetail.RankingCategoryId == rankingCategoryId);
            }

            if (allowListener.HasValue)
            {
                query = query.Where(c => c.ResearchConferenceDetail != null &&
                                         c.ResearchConferenceDetail.AllowListener == allowListener.Value);
            }

            if (totalRevisionRound.HasValue)
            {
                query = query.Where(c => c.ResearchConferenceDetail != null &&
                                         c.ResearchConferenceDetail.RevisionAttemptAllowed == totalRevisionRound.Value);
            }

            if (noReviewerFee.HasValue)
            {
                if (noReviewerFee.Value) // Muốn tìm cái ReviewFee = 0
                    query = query.Where(c => c.ResearchConferenceDetail != null &&
                                             c.ResearchConferenceDetail.ReviewFee == 0);
                else // Muốn tìm cái có phí
                    query = query.Where(c => c.ResearchConferenceDetail != null &&
                                             c.ResearchConferenceDetail.ReviewFee > 0);
            }

            // 5. Filter Chi tiết Technical
            if (!string.IsNullOrEmpty(targetAudience))
            {
                var lowerAudience = targetAudience.ToLower();
                query = query.Where(c => c.TechnicalConferenceDetail != null &&
                                         c.TechnicalConferenceDetail.TargetAudience.ToLower().Contains(lowerAudience));
            }


            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = pagedConferences.Select(conference => new ConferenceWithPricesResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                targetAudience = conference.TechnicalConferenceDetail != null ? conference.TechnicalConferenceDetail.TargetAudience : null,
                ResearchConferenceDetailResponse = conference.ResearchConferenceDetail != null ? conference.ResearchConferenceDetail.ToResearchDetailForWithPriceEndpoint() : null,
                ConferencePrices = conference.ConferencePrices?.Select(cp => new DTOs.Conference.ConferencePriceWithPhasesResponse
                {
                    ConferencePriceId = cp.ConferencePriceId,
                    TicketPrice = cp.TicketPrice,
                    TicketName = cp.TicketName,
                    TicketDescription = cp.TicketDescription,
                    IsAuthor = cp.IsAuthor,
                    TotalSlot = cp.TotalSlot,
                    AvailableSlot = cp.AvailableSlot,
                    PricePhases = cp.PricePhases?.Select(pp => new DTOs.Conference.PricePhaseResponse
                    {
                        PricePhaseId = pp.PricePhaseId,
                        PhaseName = pp.PhaseName,
                        StartDate = pp.StartDate,
                        EndDate = pp.EndDate,
                        ApplyPercent = pp.ApplyPercent,
                        TotalSlot = pp.TotalSlot,
                        AvailableSlot = pp.AvailableSlot,
                        RefundPolicies = pp?.RefundPolicies.Select(rp => rp.ToRefundPolicyResponse()).OrderBy(rp => rp.RefundOrder).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            return new PagedResult<ConferenceWithPricesResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        //Only get technical detail for anon use this so only allow onhold,ready,complete, cancel status
        public async Task<TechnicalConferenceDetailResponse> GetTechnicalConferenceDetailAsync(string conferenceId, string? userId)
        {
            string ticketId = "", pricePhaseId = "", conferencePriceId = "";
            if (userId != null)
            {
                var ticket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(userId, conferenceId);
                ticketId = ticket?.TicketId;
                pricePhaseId = ticket?.PricePhaseId;
                conferencePriceId = ticket?.PricePhase?.ConferencePrice?.ConferencePriceId;
            }


            var readystatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Ready.GetDescription());


            var conference = await _unitOfWork.ConferenceRepository.GetTechnicalIncludedById(conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            if (conference.IsResearchConference == true)
                throw new Exception("chức năng chỉ dành cho tech");

            if(
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Ready.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.OnHold.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Cancelled.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Completed.GetDescription()
               )
            {
                throw new BadRequestException($"Hội thảo đang ở trạng thái không khả dụng để xem được chi tiết");
            }

            // Get technical conference detail if it exists (for technical conferences)
            var technicalDetail = conference.TechnicalConferenceDetail;

            return new TechnicalConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                createdBy = conference.CreatedBy,
                UserNameCreator = conference.CreatedByNavigation.FullName,
                Contract = conference.CollaboratorContract != null ? conference.CollaboratorContract.toCollaboratorContractResponseForConferenceDetail() : null,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                TargetAudience = technicalDetail?.TargetAudience,
                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = conference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),
                CategoryName = conference.ConferenceCategory?.ConferenceCategoryName ?? "N/A",
                CityName = conference.City?.CityName ?? "N/A",
                StatusName = conference.ConferenceStatus?.ConferenceStatusName ?? "N/A",
                purchasedInfo = new PurchasedInfo
                {
                    ticketId = ticketId,
                    conferencePriceId = conferencePriceId,
                    pricePhaseId = pricePhaseId
                }
            };
        }

        public async Task<PagedResult<ConferenceResponseDTO>> GetConferencesByStatusAsync(string conferenceStatusId, int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .Where(c => c.ConferenceStatusId == conferenceStatusId);

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) || c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = pagedConferences.Select(conference => new ConferenceResponseDTO
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId
            }).ToList();

            return new PagedResult<ConferenceResponseDTO>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ConferenceStepCompletionStatusResponse>> GetTechnicalConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _unitOfWork.ConferenceRepository.GetAllConferences().Where(c => c.IsResearchConference == false);

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) || c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = new List<ConferenceStepCompletionStatusResponse>();

            foreach (var conference in pagedConferences)
            {
                // Check each step completion status
                var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conference.ConferenceId);
                var havePolicy = policies.Any();

                var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conference.ConferenceId);
                var haveSponsor = sponsors.Any();

                var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conference.ConferenceId);
                var haveSession = sessions.Any();

                var conferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conference.ConferenceId);
                var haveConferencePrice = conferencePrices.Any();

                var haveTechnicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conference.ConferenceId) != null;

                var haveSessionMedia = false;
                var haveSpeakerInSession = false;

                foreach (var session in sessions)
                {
                    var sessionMedia = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(session.ConferenceSessionId);
                    if (sessionMedia.Any())
                    {
                        haveSessionMedia = true;
                    }

                    var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(session.ConferenceSessionId);
                    if (speakers.Any())
                    {
                        haveSpeakerInSession = true;
                    }

                    // Break if both are confirmed true to save queries
                    if (haveSessionMedia && haveSpeakerInSession)
                    {
                        break;
                    }
                }

                responses.Add(new ConferenceStepCompletionStatusResponse
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    IsResearch = conference.IsResearchConference ?? true, // Default to true (research) if null
                    HavePolicy = havePolicy,
                    HaveSponsor = haveSponsor,
                    HaveSession = haveSession,
                    HaveSessionMedia = haveSessionMedia,
                    HaveSpeakerInSession = haveSpeakerInSession,
                    HaveConferencePrice = haveConferencePrice,
                    HaveTechnicalConferenceDetail = haveTechnicalDetail,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    CityId = conference.CityId,
                    ConferenceCategoryId = conference.ConferenceCategoryId,
                    ConferenceStatusId = conference.ConferenceStatusId
                });
            }

            return new PagedResult<ConferenceStepCompletionStatusResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // NEW ENDPOINTS IMPLEMENTATION 5 & 6

        public async Task<PagedResult<ConferenceResponseDTO>> GetPendingConferencesAsync(int page, int pageSize, string? searchKeyword = null)
        {
            // Get the "Pending" status ID first
            var allStatuses = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
            var pendingStatus = allStatuses.FirstOrDefault(s => s.ConferenceStatusName == "Pending");

            if (pendingStatus == null)
            {
                return new PagedResult<ConferenceResponseDTO>
                {
                    Items = new List<ConferenceResponseDTO>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var query = _unitOfWork.ConferenceRepository.GetAllConferences().Include(c => c.CreatedByNavigation)
                .Where(c => c.ConferenceStatusId == pendingStatus.ConferenceStatusId);

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) || c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = pagedConferences.Select(conference => new ConferenceResponseDTO
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                CreatedBy = conference.CreatedBy,
                userNameCreator = conference.CreatedByNavigation?.FullName,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId
            }).ToList();

            return new PagedResult<ConferenceResponseDTO>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ApproveConferenceAsync(string conferenceId, ApproveConferenceRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new BadRequestException($"Không tìm thấy conf id {conferenceId} này");
            var creator = conference.CreatedByNavigation;
            if (creator == null) throw new BadRequestException($"Không tìm thấy user tạo conference {conference.CreatedBy}");


            // Change conference status from Pending to Rejected
            if (request.IsApprove == false)
            {
                bool rejectedResult = await UpdateConferenceStatusAsync(conferenceId, "Rejected", request.Reason);
                if (rejectedResult)
                {
                    await SendConferenceApprovalNotification(creator, conference, false);
                }
                return rejectedResult;
            }
            // Change conference status from Pending to Preparing
            bool approveResult = await UpdateConferenceStatusAsync(conferenceId, "Preparing", request.Reason);
            if (approveResult)
            {
                await SendConferenceApprovalNotification(creator, conference, true);
            }
            return approveResult;
        }

        private async Task SendConferenceApprovalNotification(User creator, Conference conference, bool isApproved)
        {
            var timeNow = await _timeProviderService.GetVietnamTime();

            string title = "Kết quả duyệt hội nghị";
            string message = isApproved ? $"Hội nghị {conference.ConferenceName} đã được xét duyệt." : $"Hội nghị {conference.ConferenceName} đã bị từ chối.";


            var notification = new Notification
            {
                NotificationId = Guid.NewGuid().ToString(),
                UserId = creator.UserId,
                Title = title,
                Message = message,
                CreatedAt = timeNow,
                ReadStatus = false
            };

            int notiResult = await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);
            if (notiResult > 0)
            {
                if (!string.IsNullOrWhiteSpace(creator.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(creator.FirebaseMobileFcmToken, title, message);
                }

                if (!string.IsNullOrWhiteSpace(creator.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(creator.FirebaseWebFcmToken, title, message);
                }
            }
        }

        public async Task<bool> ChangeConferenceStatus(string userId, string conferenceId, string newStatus, string? reason = null)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new BadRequestException($"Không tìm thấy hội nghị với id {conferenceId}");
            if (conference.CreatedBy != userId) throw new BadRequestException("Chỉ có nguời tạo ra conference mới thay đổi được trạng thái");

            //Collaborator's technical confs need to be approved to preparing status first only then they can change the status
            var newStatusEntity = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(newStatus);
            if (newStatusEntity == null) throw new BadRequestException($"Không tìm thấy conference status vứi ID {newStatus}");

            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            var deleteStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Deleted.GetDescription());
            var rejectStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Rejected.GetDescription());

            //get disable status

            var disabledStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Disabled.GetDescription());



            //Can't use disable status in this method

            if (newStatusEntity.ConferenceStatusId == disabledStatus.ConferenceStatusId || conference.ConferenceStatusId == disabledStatus.ConferenceStatusId)
                throw new Exception("Không thể sử dụng với disabled status ở đây xin vui lòng sử dụng các endpoint ransition-conference-from-disable-to-ready và disable-conference");
            //from pending can only go delete or back to draft

            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId && (newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != draftStatus.ConferenceStatusId))

                throw new Exception("Conference cần Organizer approve lên preparing trước để có thể thay đổi trạng thái hoặc về draft để tiếp tục chỉnh sửa");

            //from draft the collaborator can only be transitioned to delete on this method, need to go the request to be approve to go to the pending
            if (conference.ConferenceStatusId == draftStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId)
                throw new Exception("Hiện tại bản draft của conference chỉ có thể chuyển sang delete. Conference cần request lên pending để Organizer approve lên preparing trước khi có thể thay đổi trạng thái khác");

            //from reject can only transitioned to draft
            if (conference.ConferenceStatusId == rejectStatus.ConferenceStatusId && (newStatusEntity.ConferenceStatusId != draftStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId))
                throw new Exception("Trạng thái hiện tại của hội nghị là rejected chỉ có thể đổi lên draft để tiếp tục sửa đổi hoặc xoá thành delete");

            //collab can switch their conf to deleted only when the associated contract be invalid will the conf status be deleted
            if (conference.IsInternalHosted != true && conference.ConferenceStatusId == deleteStatus.ConferenceStatusId)
                throw new Exception("Hội nghị được liên kết không thể chuyển sang trạng thái bị xoá, chỉ có thể tự động chuyển sang trạng thái này khi hợp đồng bị huỷ");

            return UpdateConferenceStatusAsync(conferenceId, newStatusEntity.ConferenceStatusName!, reason).Result;
        }



        //only customer to use this so only allow following status: ready, omhold,cancelled, complete
        public async Task<DTOs.Conference.ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId, string? userId)
        {
            string ticketId = "", pricePhaseId = "", conferencePriceId = "";
            SubmittedPaperInfo submittedPaperInfo = new SubmittedPaperInfo();
            // Get the main conference with related data
            var conference = await _unitOfWork.ConferenceRepository.GetResearchIncludedById(conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference với ID {conferenceId} không tìm thấy");
            }

            if (
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Ready.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.OnHold.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Cancelled.GetDescription() &&
                conference.ConferenceStatus.ConferenceStatusName != ConferenceStatusEnum.Completed.GetDescription()
               )
            {
                throw new BadRequestException($"Hội nghị nghiên cứu đang ở trạng thái không khả dụng để xem được chi tiết");
            }


            if (conference.IsResearchConference == false)
                throw new Exception("chức năng chỉ dành cho research");

            if (userId != null)
            {
                var ticket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(userId, conferenceId);
                ticketId = ticket?.TicketId;
                pricePhaseId = ticket?.PricePhaseId;
                conferencePriceId = ticket?.PricePhase?.ConferencePrice?.ConferencePriceId;

                var SubmittedPaper = await _unitOfWork.PaperRepository.GetSubmittedPaperWith4PhaseStatusByConferenceIdAndRootAuthor(conferenceId, userId);
                if (SubmittedPaper != null)
                {
                    submittedPaperInfo = new SubmittedPaperInfo()
                    {
                        PaperId = SubmittedPaper.PaperId,
                        AbstractStatus = SubmittedPaper.Abstract?.GlobalStatus?.Name,
                        FullpaperStatus = SubmittedPaper.FullPaper?.ReviewStatus?.Name,
                        RevisionStatus = SubmittedPaper.RevisionPaper?.GlobalStatus?.Name,
                        CameraReadyStatus = SubmittedPaper.CameraReady?.GlobalStatus?.Name,
                        ResearchPhaseId = SubmittedPaper.ResearchConferencePhaseId
                    };
                }
            }


            // Map to response DTO
            return new DTOs.Conference.ResearchConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                createdBy = conference.CreatedBy,
                UserNameCreator = conference.CreatedByNavigation.FullName,
                CategoryName = conference.ConferenceCategory?.ConferenceCategoryName ?? "N/A",
                CityName = conference.City?.CityName ?? "N/A",
                StatusName = conference.ConferenceStatus?.ConferenceStatusName ?? "N/A",
                
                // Research Conference Detail specific fields
                PaperFormat = conference.ResearchConferenceDetail.PaperFormat,
                NumberPaperAccept = conference.ResearchConferenceDetail?.NumberPaperAccept,
                RevisionAttemptAllowed = conference?.ResearchConferenceDetail?.RevisionAttemptAllowed,
                RankingDescription = conference?.ResearchConferenceDetail?.RankingDescription,
                AllowListener = conference?.ResearchConferenceDetail?.AllowListener,
                RankValue = conference?.ResearchConferenceDetail?.RankValue,
                RankYear = conference?.ResearchConferenceDetail?.RankYear,
                ReviewFee = conference?.ResearchConferenceDetail?.ReviewFee,
                RankingCategoryId = conference?.ResearchConferenceDetail?.RankingCategoryId,
                RankingCategoryName = conference?.ResearchConferenceDetail?.RankingCategory?.RankName,

                // Research Conference related data
                RankingFileUrls = conference.RankingFileUrls?.Select(r => r.ToRankingFileUrlResponse()).ToList(),
                MaterialDownloads = conference.MaterialDownloads?.Select(m => m.ToMaterialDownloadResponse()).ToList(),
                RankingReferenceUrls = conference.RankingReferenceUrls?.Select(r => r.ToRankingReferenceUrlResponse()).ToList(),
                ResearchPhase = conference.ResearchConferencePhases != null ? conference.ResearchConferencePhases.Select(researchPhase => researchPhase.toResearchPhaseResponse()).OrderBy(researchPhase => researchPhase.PhaseOrder).ToList() : null,
                ResearchSessions = conference.ConferenceSessions?.Select(rs => rs.ToResearchSessionWithMediaResponse()).OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList(),

                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),

                ConferenceMedia = conference.ConferenceMedia?.Select(cm => cm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),
                purchasedInfo = new PurchasedInfo
                {
                    ticketId = ticketId,
                    conferencePriceId = conferencePriceId,
                    pricePhaseId = pricePhaseId
                },
                submittedPaper = submittedPaperInfo != null ? submittedPaperInfo : null
            };
        }

        public async Task<DTOs.Conference.ResearchConferenceDetailResponse> GetDetailResearchForOrganizerAsync(string conferenceId)
        {
            // Get the main conference with related data and timeline
            var conference = await _unitOfWork.ConferenceRepository.GetResearchIncludedById(conferenceId);


            if (conference == null)
            {
                throw new NotFoundException($"Conference với ID {conferenceId} không tìm thấy");
            }

            if (conference.IsResearchConference == false)
                throw new Exception("chức năng chỉ dành cho research");


            // Map to response DTO
            return new DTOs.Conference.ResearchConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                createdBy = conference.CreatedBy,
                UserNameCreator = conference.CreatedByNavigation.FullName,
                CategoryName = conference.ConferenceCategory.ConferenceCategoryName,
                CityName = conference.City.CityName,
                StatusName = conference.ConferenceStatus.ConferenceStatusName,

                // Research Conference Detail specific fields

                PaperFormat = conference.ResearchConferenceDetail.PaperFormat,
                NumberPaperAccept = conference.ResearchConferenceDetail?.NumberPaperAccept,
                RevisionAttemptAllowed = conference?.ResearchConferenceDetail?.RevisionAttemptAllowed,
                RankingDescription = conference?.ResearchConferenceDetail?.RankingDescription,
                AllowListener = conference?.ResearchConferenceDetail?.AllowListener,
                RankValue = conference?.ResearchConferenceDetail?.RankValue,
                RankYear = conference?.ResearchConferenceDetail?.RankYear,
                ReviewFee = conference?.ResearchConferenceDetail?.ReviewFee,
                RankingCategoryId = conference?.ResearchConferenceDetail?.RankingCategoryId,
                RankingCategoryName = conference?.ResearchConferenceDetail?.RankingCategory?.RankName,

                // Research Conference related data
                RankingFileUrls = conference.RankingFileUrls?.Select(r => r.ToRankingFileUrlResponse()).ToList(),
                MaterialDownloads = conference.MaterialDownloads?.Select(m => m.ToMaterialDownloadResponse()).ToList(),
                RankingReferenceUrls = conference.RankingReferenceUrls?.Select(r => r.ToRankingReferenceUrlResponse()).ToList(),
                ResearchPhase = conference.ResearchConferencePhases != null ? conference.ResearchConferencePhases.Select(researchPhase => researchPhase.toResearchPhaseResponse()).OrderBy(researchPhase => researchPhase.PhaseOrder).ToList() : null,
                ResearchSessions = conference.ConferenceSessions?.Select(rs => rs.ToResearchSessionWithMediaResponse()).OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList(),

                // Shared tables data (same as technical conference)
                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cm => cm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),

                // Include conference timeline data
                ConferenceTimelines = conference.ConferenceTimelines?.Select(ct => ct.ToConferenceTimelineResponse()).ToList()
            };
        }

        public async Task<DTOs.Conference.TechnicalConferenceDetailResponse> GetDetailTechnicalAsync(string conferenceId, string? userId, bool isOrganizer = false)
        {
            // Check if the user is authorized to access this conference
            var conference = await _unitOfWork.ConferenceRepository.GetTechnicalIncludedById(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            if (conference.IsResearchConference == true)
                throw new Exception("Chức năng chỉ dành cho tech");



            // If the user is not an organizer, verify that they created the conference
            if (!isOrganizer)
            {
                if (conference.CreatedBy != userId)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có thể thấy detail cho conference bạn tạo ra.");
                }
            }


            return new DTOs.Conference.TechnicalConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                TargetAudience = conference.TechnicalConferenceDetail?.TargetAudience,
                createdBy = conference.CreatedBy,
                UserNameCreator = conference.CreatedByNavigation?.FullName,
                Organization = conference.CreatedByNavigation?.Organization?.OrganizationName,
                Contract = conference.CollaboratorContract != null ? conference.CollaboratorContract.toCollaboratorContractResponseForConferenceDetail() : null,
                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = conference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),

                // Include conference timeline data
                ConferenceTimelines = conference.ConferenceTimelines?.Select(ct => ct.ToConferenceTimelineResponse()).ToList(),

            };
        }

        public async Task<PagedResult<DTOs.Conference.ResearchConferenceStepCompletionStatusResponse>> GetResearchConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            // Only get research conferences
            var query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .Where(c => c.IsResearchConference == true);

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) || c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = new List<DTOs.Conference.ResearchConferenceStepCompletionStatusResponse>();

            foreach (var conference in pagedConferences)
            {
                // Check each step completion status for research conferences
                var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conference.ConferenceId);
                var haveResearchConferenceDetail = researchDetail != null;

                var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conference.ConferenceId);
                var haveMaterialDownload = materialDownloads.Any();

                var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conference.ConferenceId);
                var haveRankingFileUrl = rankingFileUrls.Any();

                var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conference.ConferenceId);
                var haveRankingReferenceUrl = rankingReferenceUrls.Any();

                var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conference.ConferenceId);
                var haveResearchPhase = researchPhase != null;

                var researchSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conference.ConferenceId);
                var haveResearchSession = researchSessions.Any();

                var haveResearchSessionMedia = false;
                foreach (var session in researchSessions)
                {
                    var sessionMedia = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(session.ConferenceSessionId);
                    if (sessionMedia.Any())
                    {
                        haveResearchSessionMedia = true;
                        break;
                    }
                }

                var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conference.ConferenceId);
                var havePolicy = policies.Any();

                var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conference.ConferenceId);
                var haveSponsor = sponsors.Any();

                var conferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conference.ConferenceId);
                var haveConferencePrice = conferencePrices.Any();

                var refundPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByConferenceIdAsync(conference.ConferenceId);
                var haveRefundPolicy = refundPolicies.Any();

                var conferenceMedia = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conference.ConferenceId);
                var haveConferenceMedia = conferenceMedia.Any();

                responses.Add(new DTOs.Conference.ResearchConferenceStepCompletionStatusResponse
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    IsResearch = true, // Always true for research conferences
                    HaveResearchConferenceDetail = haveResearchConferenceDetail,
                    HaveMaterialDownload = haveMaterialDownload,
                    HaveRankingFileUrl = haveRankingFileUrl,
                    HaveRankingReferenceUrl = haveRankingReferenceUrl,
                    HaveResearchPhase = haveResearchPhase,
                    HaveResearchSession = haveResearchSession,
                    HaveResearchSessionMedia = haveResearchSessionMedia,
                    HavePolicy = havePolicy,
                    HaveSponsor = haveSponsor,
                    HaveConferencePrice = haveConferencePrice,
                    HaveRefundPolicy = haveRefundPolicy,
                    HaveConferenceMedia = haveConferenceMedia,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    CityId = conference.CityId,
                    ConferenceCategoryId = conference.ConferenceCategoryId,
                    ConferenceStatusId = conference.ConferenceStatusId
                });
            }

            return new PagedResult<DTOs.Conference.ResearchConferenceStepCompletionStatusResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> CheckTechnicalConferenceStepCompletionAsync(string conferenceId, string step)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                return false;
            }

            // Only check for technical conferences
            if (conference.IsResearchConference == true)
            {
                return false;
            }

            switch (step.ToLower())
            {
                case "technicalconference":
                    // This is always true as the conference exists
                    return true;
                case "technicalconferencedetail":
                    var technicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conferenceId);
                    return technicalDetail != null;
                case "policy":
                    var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
                    return policies.Any();
                case "sponsor":
                    var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
                    return sponsors.Any();
                case "session":
                    var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
                    return sessions.Any();
                case "sessionmedia":
                    var sessionsForMedia = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
                    foreach (var session in sessionsForMedia)
                    {
                        var sessionMedia = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(session.ConferenceSessionId);
                        if (sessionMedia.Any())
                            return true;
                    }
                    return false;
                case "speaker":
                    var sessionsForSpeakers = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
                    foreach (var session in sessionsForSpeakers)
                    {
                        var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(session.ConferenceSessionId);
                        if (speakers.Any())
                            return true;
                    }
                    return false;
                case "price":
                    var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
                    return prices.Any();
                case "refundpolicy":
                    var refundPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByConferenceIdAsync(conferenceId);
                    return refundPolicies.Any();
                case "conferencemedia":
                    var conferencemedias = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conferenceId);
                    return conferencemedias.Any();
                default:
                    return false;
            }
        }

        public async Task<bool> CheckResearchConferenceStepCompletionAsync(string conferenceId, string step)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                return false;
            }

            // Only check for research conferences
            if (conference.IsResearchConference != true)
            {
                return false;
            }

            switch (step.ToLower())
            {
                case "researchconference":
                    // This is always true as the conference exists
                    return true;
                case "researchconferencedetail":
                    var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);
                    return researchDetail != null;
                case "materialdownload":
                    var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conferenceId);
                    return materialDownloads.Any();
                case "rankingfileurl":
                    var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
                    return rankingFileUrls.Any();
                case "rankingreferenceurl":
                    var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
                    return rankingReferenceUrls.Any();
                case "researchphase":
                    var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conferenceId);
                    return researchPhase != null;
                case "researchsession":
                    var researchSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
                    return researchSessions.Any();
                case "researchsessionmedia":
                    var researchSessionsForMedia = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
                    foreach (var session in researchSessionsForMedia)
                    {
                        var sessionMedia = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(session.ConferenceSessionId);
                        if (sessionMedia.Any())
                            return true;
                    }
                    return false;
                case "policy":
                    var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
                    return policies.Any();
                case "sponsor":
                    var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
                    return sponsors.Any();
                case "price":
                    var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
                    return prices.Any();
                case "refundpolicy":
                    var refundPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByConferenceIdAsync(conferenceId);
                    return refundPolicies.Any();
                case "conferencemedia":
                    var conferenceMedia = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conferenceId);
                    return conferenceMedia.Any();
                default:
                    return false;
            }
        }

        public async Task<PagedResult<DTOs.Conference.ResearchConferenceDetailResponse>> GetResearchConferencesListAsync(
            int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
            string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
            string? userId = null, bool isOrganizer = false)
        {
            IQueryable<Conference> query;

            if (isOrganizer)
            {
                // Organizers can see all research conferences
                query = _unitOfWork.ConferenceRepository.GetAllConferences()
                    .Where(c => c.IsResearchConference == true && c.CreatedBy == userId).OrderByDescending(c => c.CreatedAt);
            }
            else
            {
                // Collaborators can only see research conferences they created
                query = _unitOfWork.ConferenceRepository.GetAllConferences()
                    .Where(c => c.IsResearchConference == true && c.CreatedBy == userId).OrderByDescending(c => c.CreatedAt);
            }

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(conferenceStatusId))
            {
                query = query.Where(c => c.ConferenceStatusId == conferenceStatusId);
            }

            // Apply other filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) ||
                                        c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = new List<DTOs.Conference.ResearchConferenceDetailResponse>();

            foreach (var conference in pagedConferences)
            {
                // For each conference, get the detailed research conference data
                var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conference.ConferenceId);
                var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conference.ConferenceId);
                var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conference.ConferenceId);
                var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conference.ConferenceId);
                var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conference.ConferenceId);
                var researchSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdWithRoomAsync(conference.ConferenceId);
                var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conference.ConferenceId);
                var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conference.ConferenceId);
                var conferencePrices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conference.ConferenceId);
                var refundPolicies = await _unitOfWork.ConferenceRefundPolicyRepository.GetRefundPoliciesByConferenceIdAsync(conference.ConferenceId);
                var conferenceMedia = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conference.ConferenceId);

                var response = new DTOs.Conference.ResearchConferenceDetailResponse
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    Description = conference.Description,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    TotalSlot = conference.TotalSlot,
                    AvailableSlot = conference.AvailableSlot,
                    Address = conference.Address,
                    BannerImageUrl = conference.BannerImageUrl,
                    CreatedAt = conference.CreatedAt,
                    TicketSaleStart = conference.TicketSaleStart,
                    TicketSaleEnd = conference.TicketSaleEnd,
                    IsInternalHosted = conference.IsInternalHosted,
                    IsResearchConference = conference.IsResearchConference,
                    CityId = conference.CityId,
                    ConferenceCategoryId = conference.ConferenceCategoryId,
                    ConferenceStatusId = conference.ConferenceStatusId,
                    createdBy = conference.CreatedBy,

                    // Research Conference Detail specific fields
                    PaperFormat = researchDetail?.PaperFormat,
                    NumberPaperAccept = researchDetail?.NumberPaperAccept,
                    RevisionAttemptAllowed = researchDetail?.RevisionAttemptAllowed,
                    RankingDescription = researchDetail?.RankingDescription,
                    AllowListener = researchDetail?.AllowListener,
                    RankValue = researchDetail?.RankValue,
                    RankYear = researchDetail?.RankYear,
                    ReviewFee = researchDetail?.ReviewFee,
                    RankingCategoryId = researchDetail?.RankingCategoryId,
                    RankingCategoryName = researchDetail?.RankingCategory?.RankName,

                    // Research Conference related data
                    RankingFileUrls = rankingFileUrls?.Select(r => r.ToRankingFileUrlResponse()).ToList(),
                    MaterialDownloads = materialDownloads?.Select(m => m.ToMaterialDownloadResponse()).ToList(),
                    RankingReferenceUrls = rankingReferenceUrls?.Select(r => r.ToRankingReferenceUrlResponse()).ToList(),
                    ResearchPhase = researchPhase != null ? researchPhase.Select(researchPhase => researchPhase.toResearchPhaseResponse()).OrderBy(researchPhase => researchPhase.PhaseOrder).ToList() : null,
                    ResearchSessions = researchSessions?.Select(rs => rs.ToResearchSessionWithMediaResponse()).ToList(),

                    // Shared tables data (same as technical conference)
                    Policies = policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                    Sponsors = sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                    //RefundPolicies = refundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                    //{
                    //    RefundPolicyId = rp.RefundPolicyId,
                    //    PercentRefund = rp.PercentRefund,
                    //    RefundDeadline = rp.RefundDeadline,
                    //    RefundOrder = rp.RefundOrder
                    //}).ToList(),
                    ConferenceMedia = conferenceMedia?.Select(cm => cm.ToConferenceMediaResponse()).ToList(),
                    ConferencePrices = conferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList()
                };

                responses.Add(response);
            }

            return new PagedResult<DTOs.Conference.ResearchConferenceDetailResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByOrganizerAsync(
     int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
     string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
     string? userId = null, bool isOrganizer = false)
        {
            // Nếu hàm này chỉ dành cho Organizer, ta có thể bỏ qua check bool isOrganizer 
            // vì Controller đã Authorize Role rồi. Nhưng giữ lại để double-check cũng tốt.

            if (!isOrganizer)
            {
                throw new Exception("Chức năng này chỉ dành cho Organizer.");
            }

            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            var deleteStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Deleted.GetDescription());

            // Validation
            if (!string.IsNullOrEmpty(conferenceStatusId) && (conferenceStatusId == draftStatus.ConferenceStatusId || conferenceStatusId == deleteStatus.ConferenceStatusId))
            {
                throw new BadRequestException("Organizers không được phép xem chọn lọc theo trạng thái 'Draft' và 'Deleted'.");
            }

            // 1. Khởi tạo Query với đầy đủ Include ngay từ đầu
            // Thay vì GetAllConferences(), hãy dùng hàm đã Include sẵn
            var query = _unitOfWork.ConferenceRepository.GetAllTechnicalIncludedConference();

            // 2. Apply Filters cơ bản
            query = query.Where(c => c.IsResearchConference != true // Tương đương (false || null)
                                  && c.ConferenceStatusId != draftStatus.ConferenceStatusId
                                  && c.ConferenceStatusId != deleteStatus.ConferenceStatusId
                                  && c.CreatedBy == userId);

            // 3. Apply Dynamic Filters
            if (!string.IsNullOrEmpty(conferenceStatusId))
            {
                query = query.Where(c => c.ConferenceStatusId == conferenceStatusId);
            }

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var lowerKeyword = searchKeyword.ToLower();
                query = query.Where(c => c.ConferenceName.ToLower().Contains(lowerKeyword) ||
                                         c.Description.ToLower().Contains(lowerKeyword));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            // 4. Thực thi Query (Chỉ 2 câu lệnh SQL: 1 đếm tổng, 1 lấy dữ liệu phân trang)
            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Mapping dữ liệu (Trong bộ nhớ RAM, không gọi DB nữa)
            var responses = pagedConferences.Select(fullConference => new DTOs.Conference.TechnicalConferenceDetailResponse
            {
                ConferenceId = fullConference.ConferenceId,
                ConferenceName = fullConference.ConferenceName,
                Description = fullConference.Description,
                StartDate = fullConference.StartDate,
                EndDate = fullConference.EndDate,
                TotalSlot = fullConference.TotalSlot,
                AvailableSlot = fullConference.AvailableSlot,
                Address = fullConference.Address,
                BannerImageUrl = fullConference.BannerImageUrl,
                CreatedAt = fullConference.CreatedAt,
                TicketSaleStart = fullConference.TicketSaleStart,
                TicketSaleEnd = fullConference.TicketSaleEnd,
                IsInternalHosted = fullConference.IsInternalHosted,
                IsResearchConference = fullConference.IsResearchConference,
                CityId = fullConference.CityId,
                ConferenceCategoryId = fullConference.ConferenceCategoryId,
                ConferenceStatusId = fullConference.ConferenceStatusId,

                // Dữ liệu này đã được Include sẵn, không cần query lại
                TargetAudience = fullConference.TechnicalConferenceDetail?.TargetAudience,
                createdBy = fullConference.CreatedBy,
                UserNameCreator = fullConference.CreatedByNavigation.FullName,

                // Mapping các list con
                Policies = fullConference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = fullConference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = fullConference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).ToList(),
                ConferenceMedia = fullConference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = fullConference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList()
            }).ToList();

            return new PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        public async Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorAsync(
          int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
          string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
          string? userId = null, bool isOrganizer = false, string collboratorId = null, string? organization = null, bool excludeDraft = false)
        {
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            var deleteStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Deleted.GetDescription());
            var query = _unitOfWork.ConferenceRepository.GetAllTechnicalIncludedConference();

            // 1. Filter cơ bản cho Technical Conference (Loại bỏ Research)
            query = query.Where(c => c.IsResearchConference != true); // Tương đương: null hoặc false


            if (isOrganizer && !string.IsNullOrEmpty(conferenceStatusId) && (conferenceStatusId == draftStatus.ConferenceStatusId || conferenceStatusId == deleteStatus.ConferenceStatusId))
            {
                throw new BadRequestException("Organizers không được phép xem chọn lọc theo trạng thái 'Draft' và 'Deleted'.");
            }
            if (isOrganizer)
            {
                // Organizers can see all technical conferences
                query = query
                    .Where(c => c.ConferenceStatusId != draftStatus.ConferenceStatusId &&
                    c.ConferenceStatusId != deleteStatus.ConferenceStatusId &&
                    c.CreatedBy != userId);
                if (!string.IsNullOrEmpty(collboratorId))
                    query = query.Where(c => c.CreatedBy == collboratorId);

                if (!string.IsNullOrEmpty(organization))
                {
                    // Lưu ý: Cần chắc chắn Organization không null để tránh lỗi NullReferenceException trong DB query
                    query = query.Where(c => c.CreatedByNavigation.Organization != null &&
                                             c.CreatedByNavigation.Organization.OrganizationName.ToLower() == organization.ToLower());
                }

            }

            else
            {
                if (conferenceStatusId == deleteStatus.ConferenceStatusId)
                    throw new BadRequestException("Collaborator không được phép xem chọn lọc theo trạng thái 'Deleted'.");
                // Collaborators can only see technical conferences they created
                query = query.Where(c => c.CreatedBy == userId && c.ConferenceStatusId != deleteStatus.ConferenceStatusId);
            }

            if (excludeDraft)
            {
                query = query.Where(c => c.ConferenceStatusId != draftStatus.ConferenceStatusId);
            }

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(conferenceStatusId))
            {
                query = query.Where(c => c.ConferenceStatusId == conferenceStatusId);
            }


            // Apply other filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.ToLower().Contains(searchKeyword.ToLower()) ||
                                        c.Description.ToLower().Contains(searchKeyword.ToLower()));
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                query = query.Where(c => c.CityId == cityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate);
            }

            query = query.Where(c => c.CollaboratorContract != null);

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = pagedConferences.Select(conference => new DTOs.Conference.TechnicalConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                TargetAudience = conference.TechnicalConferenceDetail?.TargetAudience,
                UserNameCreator = conference.CreatedByNavigation?.FullName, // Null check an toàn
                Organization = conference.CreatedByNavigation?.Organization?.OrganizationName, // Null check an toàn
                createdBy = conference.CreatedBy,

                // Mapping lists
                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = conference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList()
            }).ToList();
            return new PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }




        public async Task<List<ConferenceWithStatusNameResponse>> GetAllConferenceWithStatusByUserId(string userId, string? statusId)
        {
            // Get conferences for the user and by status
            var conferences = await _unitOfWork.ConferenceRepository.GetConferencesByUserIdAndStatusAsync(userId, statusId);
            conferences = conferences.OrderByDescending(c => c.CreatedAt).ToList();

            var responses = new List<ConferenceWithStatusNameResponse>();

            foreach (var conference in conferences)
            {
                var conferenceStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conference.ConferenceStatusId);

                var response = new ConferenceWithStatusNameResponse
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    Description = conference.Description,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    TotalSlot = conference.TotalSlot,
                    AvailableSlot = conference.AvailableSlot,
                    Address = conference.Address,
                    BannerImageUrl = conference.BannerImageUrl,
                    CreatedAt = conference.CreatedAt,
                    TicketSaleStart = conference.TicketSaleStart,
                    TicketSaleEnd = conference.TicketSaleEnd,
                    IsInternalHosted = conference.IsInternalHosted,
                    IsResearchConference = conference.IsResearchConference,
                    CityId = conference.CityId,
                    CreatedBy = conference.CreatedBy,
                    ConferenceCategoryId = conference.ConferenceCategoryId,
                    ConferenceStatusName = conferenceStatus?.ConferenceStatusName // Use status name instead of ID
                };

                responses.Add(response);
            }

            return responses;
        }

        public async Task<int> SubmitConferenceFeedback(CreateConferenceFeedbackRequest request, string userId)
        {
            var conferenceSession = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(request.ConferenceSessionId);
            if (conferenceSession == null)
            {
                throw new BadRequestException($"Không tìm thấy phiên với mã {request.ConferenceSessionId}");
            }
            var userCheckInFound = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(userId, request.ConferenceSessionId);
            if (userCheckInFound == null)
            {
                throw new BadRequestException($"Bạn chưa mua vé nào nên không thể dánh giá");
            }
            var checkdInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.CheckedIn.GetDescription());
            if (checkdInStatus == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái check in trong hệ thống");
            }
            if (userCheckInFound.CheckinStatusId != checkdInStatus.CheckinStatusId)
            {
                throw new BadRequestException($"Bạn phải check in rồi mới được dánh giá");
            }
            var conferenceFeedbackObj = new ConferenceFeedback()
            {
                ConferenceFeedbackId = Guid.NewGuid().ToString(),
                UserId = userId,
                ConferenceSessionId = request.ConferenceSessionId,
                Rating = request.Rating,
                Message = request.Message,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
            };
            return await _unitOfWork.ConferenceFeedbackRepository.CreateFeedbackAsync(conferenceFeedbackObj);
        }

        public async Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId)
        {
            var readyStatusConference = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var completedStatusConf = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Completed.GetDescription());
            var canceledStatusConf = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Cancelled.GetDescription());
            var onHoldStatusConf = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.OnHold.GetDescription());
            if (readyStatusConference == null || completedStatusConf ==null || canceledStatusConf ==null || onHoldStatusConf==null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái ready cho hội nghị");
            }
            var confStatuses = new List<string>()
            {
                readyStatusConference.ConferenceStatusId,
                completedStatusConf.ConferenceStatusId,
                canceledStatusConf.ConferenceStatusId,
                onHoldStatusConf.ConferenceStatusId
            };

            return await _unitOfWork.ConferenceRepository.GetListConferencesForScheduleByUserId(userId, await _timeProviderService.GetVietnamDate(), confStatuses);
        }

        public async Task<List<ConferenceResponseDTO>> GetConferenceByAssignedPapers(string? userId)
        {
            List<PaperReviewer> AssignPaper = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAsync(userId);
            List<Conference> AssignedConference = AssignPaper.Select(ap => ap.Paper.Conference).Distinct().OrderByDescending(c => c.CreatedAt).ToList();
            List<ConferenceResponseDTO> responses = new();
            foreach (var conference in AssignedConference)
            {
                ConferenceResponseDTO conferenceResponse = new ConferenceResponseDTO
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    Description = conference.Description,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    TotalSlot = conference.TotalSlot,
                    AvailableSlot = conference.AvailableSlot,
                    Address = conference.Address,
                    BannerImageUrl = conference.BannerImageUrl,
                    CreatedAt = conference.CreatedAt,
                    TicketSaleStart = conference.TicketSaleStart,
                    TicketSaleEnd = conference.TicketSaleEnd,
                    IsInternalHosted = conference.IsInternalHosted,
                    IsResearchConference = conference.IsResearchConference,
                    CityId = conference.CityId,
                    CreatedBy = conference.CreatedBy,
                    ConferenceCategoryId = conference.ConferenceCategoryId,
                    ConferenceStatusId = conference.ConferenceStatusId
                };
                responses.Add(conferenceResponse);
            }
            return responses;
        }

        public async Task<bool> RequestOrganizerApproval(string confId, string userId)
        {
            //find conf
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conference == null) throw new BadRequestException($"Không tìm thấy hội nghị với ID: {confId}");

            //must be the creator to commit the act
            if (conference.CreatedBy != userId) throw new BadRequestException("Bạn không có quyền gởi yêu cầu  approve cho hội nghị này");

            //get user
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);

            //must be draft to submit the request
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            if (conference.ConferenceStatusId != draftStatus.ConferenceStatusId) throw new BadRequestException($" conference với ID {confId} phải đang là draft status mới có thể yêu cầu duyệt được");

            //if you already submit one and is waiting you can must wait first although it will never reach here since the current need to be draft first so it can't be pending anywaya
            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId) throw new BadRequestException("Hội nghị đã gửi yêu cầu được duyệt trước đó rồi xin chờ kết quả!");
            return await UpdateConferenceStatusAsync(confId, pendingStatus.ConferenceStatusName, $"Collborator với tên: {user.FullName} dang request conference với tên: {conference.ConferenceName} để được duyệt");
        }



        public async Task<bool> ActivateNextPhase(string confId, string userId)
        {


            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conference == null)
                throw new NotFoundException($"Không tìm thấy hội nghị với ID {confId}");

            //authorization
            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền kích hoạt phase waitlist cho hội nghị này.");
            if (conference.IsResearchConference != true)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị nghiên cứu.");

            // 1.2. Lấy các Phase 
            var active = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(confId);
            if (active == null || !active.PhaseOrder.HasValue) // Kiểm tra cả PhaseOrder cho chắc chắn
                throw new BadRequestException("Hội nghị không có giai đoạn nào đang hoạt động hoặc cấu hình bị lỗi.");

            var nextphase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByOrderAndConferenceIdAsync(confId, active.PhaseOrder.Value + 1);
            if (nextphase == null)
                throw new BadRequestException("Hội nghị không còn giai đoạn tiếp theo để kích hoạt.");

            // 1.3. Lấy Research Detail (cần cho các bước sau)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(confId);
            if (researchDetail == null)
                throw new InvalidOperationException($"Hội nghị chưa có chi tiết nghiên cứu (Research Detail).");

            // 1.4. Kiểm tra xem waitlist đã được kích hoạt chưa
            if (nextphase.IsActive == true)
                throw new BadRequestException($"Waitlist cho hội nghị này đã kích hoạt truớc đó.");





            #region === 2. VALIDATION LOGIC NGHIỆP VỤ ===

            // 2.1. Kiểm tra số lượng vé Author còn lại
            var authorConferencePrices = await _unitOfWork.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(confId);
            var remainingAuthorSlots = authorConferencePrices.Sum(cp => cp.AvailableSlot ?? 0);
            if (remainingAuthorSlots <= 0)
                throw new BadRequestException("Không thể kích hoạt phase tiếp theo vì tất cả các suất dành cho tác giả (vé 'IsAuthor') đã được bán hết.");


            // 2.2. Kiểm tra điều kiện thời gian
            var today = await _timeProviderService.GetVietnamDate();
            // 2.2a. Phải sau khi phase hiện tại kết thúc hoàn toàn (kết thúc AuthorPaymentEnd)
            if (today <= active.AuthorPaymentEnd)
                throw new BadRequestException($"Không thể kích hoạt phase tiếp theo khi phase hiện tại chưa kết thúc. Phase hiện tại kết thúc vào ngày: {active.AuthorPaymentEnd:dd/MM/yyyy}.");
            // 2.2b. Phải nằm trong khoảng thời gian đăng ký của phase tiếp theo
            if (today < nextphase.RegistrationStartDate || today > nextphase.RegistrationEndDate)
                throw new BadRequestException($"Chỉ có thể kích hoạt phase tiếp theo trong khoảng thời gian đăng ký của nó ({nextphase.RegistrationStartDate:dd/MM/yyyy} - {nextphase.RegistrationEndDate:dd/MM/yyyy}).");
            // 2.3. Kiểm tra xem người tổ chức đã tạo PricePhase cho vé Author trong giai đoạn Waitlist chưa
            var allAuthorPricePhases = await _unitOfWork.PricePhaseRepository.GetPricePhaseByconferenceIdThatIsAuthor(confId);
            bool hasPricePhaseForWaitlist = allAuthorPricePhases.Any(pp => pp.ResearchConferencePhaseId == nextphase.ResearchConferencePhaseId);



            if (!hasPricePhaseForWaitlist)
                throw new BadRequestException($"Không thể kích hoạt phase tiếp theo. Vui lòng tạo ít nhất một 'Giai đoạn bán vé' (Price Phase) cho loại vé 'IsAuthor' có khoảng thời gian nằm trong giai đoạn payment {nextphase.AuthorPaymentStart:dd/MM/yyyy} - {nextphase.AuthorPaymentEnd:dd/MM/yyyy} của waitlist.");
            //2.4 kiểm tra xem phase tiếp theo có đầy đủ revision round chưa
            int allowedAttempts = researchDetail.RevisionAttemptAllowed ?? 0;
            var deadlines = await _unitOfWork.RevisionRoundDeadlineRepository.GetCsByPhaseIdAsync(nextphase.ResearchConferencePhaseId); 
            if (deadlines == null ||  allowedAttempts != deadlines.Count())
            {
                throw new BadRequestException($"Không thể kích hoạt. Giai đoạn tiếp theo chưa được cấu hình đủ số vòng sửa bài. Yêu cầu: {allowedAttempts}, Hiện có: {deadlines.Count()}.");
            }

            #endregion

            #region === 3. THỰC THI THAY ÐỎI ===
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                nextphase.IsActive = true;
                active.IsActive = false;

                await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(nextphase);
                await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(active);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            #endregion
        }



        public async Task<List<SkeletonTechConfResponse>> getSkeletonTechConf(string collaboratorId)
        {
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Draft.GetDescription());
            var conferencesCreatedForCollaborator = await _unitOfWork.ConferenceRepository.GetConferencesByUserIdAndStatusAsync(collaboratorId, draftStatus.ConferenceStatusId);
            conferencesCreatedForCollaborator = conferencesCreatedForCollaborator.Where(c => c.CollaboratorContract == null && c.IsInternalHosted != true).ToList();
            return conferencesCreatedForCollaborator.Select(c => new SkeletonTechConfResponse
            {
                ConferenceId = c.ConferenceId,
                Name = c.ConferenceName,
                createdAt = c.CreatedAt
            }).ToList();
        }

        public async Task<PagedResult<TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorOnlyDraftAsync(int page, int pageSize, string? searchKeyword, string? cityId, DateOnly? startDate, DateOnly? endDate, string? userId, bool isOrganizer, string? collaboratorId, string? organizationName)
        {
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            return await GetTechnicalConferencesListByCollaboratorAsync(page, pageSize, draftStatus.ConferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer, collaboratorId, organizationName);
        }

        public async Task<PagedResult<TechnicalConferenceDetailResponse>> GetTechnicalConferencesListByCollaboratorNoDraftAsync(int page, int pageSize, string? conferenceStatusId, string? searchKeyword, string? cityId, DateOnly? startDate, DateOnly? endDate, string? userId, bool isOrganizer, string? collaboratorId, string? organizationName)
        {
            if (!string.IsNullOrEmpty(conferenceStatusId))
            {
                var checkStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conferenceStatusId);
                if (checkStatus.ConferenceStatusName == ConferenceStatusEnum.Draft.GetDescription())
                    throw new Exception("Không thể lọc theo draft ở endpoint này");
            }

            return await GetTechnicalConferencesListByCollaboratorAsync(page, pageSize, conferenceStatusId, searchKeyword, cityId, startDate, endDate, userId, isOrganizer, collaboratorId, organizationName, true);
        }

        public async Task<bool> AutoAdjustTimelineForOnHoldAsync(string conferenceId, string userId)
        {
            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conf == null) throw new NotFoundException($"Không tìm thấy hội nghị {conferenceId}");

            if (conf.CreatedBy != userId)
                throw new Exception("Bạn không có quyền thực hiện thao tác này.");

            // Check Status OnHold
            var onHoldStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.OnHold.GetDescription());
            var readyStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());

            if (conf.ConferenceStatusId != onHoldStatus.ConferenceStatusId)
            {
                throw new BadRequestException("Chức năng này chỉ khả dụng khi hội nghị đang ở trạng thái 'OnHold'.");
            }

            // Lấy lịch sử
            var onHoldTimelineEntry = await _unitOfWork.ConferenceTimelineRepository
                .GetLastTransitionConferenceTimelineByConfIdAndStatusIdAsync(conf.ConferenceId, readyStatus.ConferenceStatusId, onHoldStatus.ConferenceStatusId);

            if (onHoldTimelineEntry == null)
                throw new BadRequestException("Không tìm thấy lịch sử chuyển sang trạng thái 'OnHold' để tính toán thời gian.");

            var onHoldStartDate = onHoldTimelineEntry.ChangeDate.Value; // DateOnly (Ngày bắt đầu bị dừng)
            var today = await _timeProviderService.GetVietnamDate(); // DateOnly (Ngày hôm nay)

            int daysToShift = today.DayNumber - onHoldStartDate.DayNumber;

            if (daysToShift <= 0)
            {
                throw new BadRequestException("Thời gian OnHold chưa đủ 1 ngày hoặc ngày hiện tại không hợp lệ để tự động điều chỉnh.");
            }

            if (daysToShift <= 0) throw new BadRequestException("...");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Gọi hàm nội bộ vừa tách
                await ExecuteTimelineShiftInternal(conf, daysToShift, onHoldStartDate);
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DisableContractedConference(string confId, string? reason = null)
        {
            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conf == null)
                throw new Exception("Hội nghị không tồn tại");
            if (conf.IsInternalHosted != false)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị được liên kết");

            var readyStatus = await _conferenceStatusService.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());

            if (conf.ConferenceStatusId != readyStatus.ConferenceStatusId)
                throw new Exception("Hội nghị phải trong trạng thái ready mới có thể chuyển sang disabled");

            string ReasonForDisabling = !string.IsNullOrEmpty(reason) ? reason : $"Hội nghị {conf.ConferenceName} đã bị chuyển về trạng thái Disabled";
            await UpdateConferenceStatusAsync(confId, ConferenceStatusEnum.Disabled.GetDescription(), ReasonForDisabling);
            return true;

        }

        public async Task<bool> ToReadyFromDisabledContractedConference(string confId, string? reason)
        {
            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conf == null)
                throw new Exception("Hội nghị không tồn tại");
            if (conf.IsInternalHosted != false)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị được liên kết");

            var disabledStatus = await _conferenceStatusService.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Disabled.GetDescription());

            if (conf.ConferenceStatusId != disabledStatus.ConferenceStatusId)
                throw new Exception("Hội nghị phải trong trạng thái disabled mới có thể chuyển sang ready");

            string ReasonForReady = !string.IsNullOrEmpty(reason) ? reason : $"Hội nghị {conf.ConferenceName} đã bị chuyển về trạng thái Disabled";
            await UpdateConferenceStatusAsync(confId, ConferenceStatusEnum.Ready.GetDescription(), ReasonForReady);

            return true;
        }
    }
}