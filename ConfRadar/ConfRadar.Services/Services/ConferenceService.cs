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
        Task<PagedResult<ConferenceWithPricesResponse>> GetConferencesWithPricesAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

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

        // Helper method: Update conference status
        Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null);

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
        Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListAsync(int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null, string? userId = null, bool isOrganizer = false);

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
        Task<bool> ActivateWaitlist(string confId, string userId);
        Task ValidateForReadyStateAsync(Conference conf);
        Task OnholdToReadyValidAsync(Conference conf, string readyId, string onHoldId);


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




        private async Task<List<string>> ValidateConferenceTimelineAsync(Conference conf, Func<DateOnly?, bool> dateOnlyValidationRule)
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

            // === TẢI DỮ LIỆU LIÊN QUAN TRƯỚC ĐỂ TRÁNH LỖI N+1 QUERY ===
            // (Giả sử bạn có các phương thức repository hỗ trợ Include)
            var allPricesWithPhasesAndPolicies = await _unitOfWork.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(conf.ConferenceId);
            var allSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conf.ConferenceId);

            // 1. Kiểm tra Conference
            AddIfInvalid(conf.StartDate, "Ngày bắt đầu hội nghị");
            AddIfInvalid(conf.EndDate, "Ngày kết thúc hội nghị");
            AddIfInvalid(conf.TicketSaleEnd, "Ngày kết thúc bán vé");

            // 2. Kiểm tra PricePhase và RefundPolicy (dùng dữ liệu đã tải sẵn)
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

            // 3. Kiểm tra ConferenceSession (dùng dữ liệu đã tải sẵn)
            foreach (var session in allSessions)
            {
                AddIfInvalid(session.SessionDate, $"Phiên '{session.Title}'");
            }

            // 4. Kiểm tra Research Conference (nếu có)
            if (conf.IsResearchConference == true)
            {
                var allResearchPhases = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conf.ConferenceId);
                foreach (var phase in allResearchPhases)
                {
                    string phaseType = (phase.IsWaitlist ?? false) ? "Phase Waitlist" : "Phase Chính";

                    // SỬA LỖI LOGIC HIỂN THỊ NGÀY Ở ĐÂY
                    AddIfInvalid(phase.RegistrationEndDate, $"{phaseType}: Hạn chót đăng ký");
                    AddIfInvalid(phase.FullPaperEndDate, $"{phaseType}: Hạn chót nộp Full Paper");
                    AddIfInvalid(phase.ReviewEndDate, $"{phaseType}: Hạn chót phản biện");
                    AddIfInvalid(phase.ReviseEndDate, $"{phaseType}: Hạn chót sửa đổi");
                    AddIfInvalid(phase.CameraReadyEndDate, $"{phaseType}: Hạn chót Camera Ready");

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

        public async Task<PagedResult<ConferenceWithPricesResponse>> GetConferencesWithPricesAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            //only retrieve conference with status ready
            var readyStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName(ConferenceStatusEnum.Ready.GetDescription());
            IQueryable<Conference> query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases).ThenInclude(pp => pp.RefundPolicies)
                    .Where(c => c.ConferenceStatusId == readyStatus.ConferenceStatusId)
                    .OrderByDescending(c => c.CreatedAt);

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
                        RefundPolicies = pp?.RefundPolicies.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        {
                            RefundPolicyId = rp.RefundPolicyId,
                            PercentRefund = rp.PercentRefund,
                            PricePhaseID = rp.PricePhaseId,
                            RefundDeadline = rp.RefundDeadline,
                            RefundOrder = rp.RefundOrder
                        }).OrderBy(rp => rp.RefundOrder).ToList()
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



            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                        .ThenInclude(pp => pp.RefundPolicies)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Speakers)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.ConferenceSessionMedia)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room) // Include room information
                         .ThenInclude(r => r.Destination)
                            .ThenInclude(d => d.City)
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            if (conference.IsResearchConference == true)
                throw new Exception("chức năng chỉ dành cho tech");

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
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                TargetAudience = technicalDetail?.TargetAudience,
                //contractURL = technicalDetail?.ContractUrl,
                //commission = technicalDetail?.Commission,
                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = conference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),
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

            var query = _unitOfWork.ConferenceRepository.GetAllConferences()
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
            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId) throw new Exception("Conference cần Organizer approve lên preparing trước để có thể thay đổi trạng thái");
            if (conference.ConferenceStatusId == draftStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId) throw new Exception("Hiện tại bản draft của conference chỉ có thể chuyển sang delete. Conference cần request lên pending để Organizer approve lên preparing trước khi có thể thay đổi trạng thái khác");


            return UpdateConferenceStatusAsync(conferenceId, newStatusEntity.ConferenceStatusName!, reason).Result;
        }

        public async Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null)
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
                    if (currentStatus.ConferenceStatusName == "OnHold")
                    {
                        await OnholdToReadyValidAsync(conference, newStatus.ConferenceStatusId, currentStatus.ConferenceStatusId);
                    }
                    else await ValidateForReadyStateAsync(conference);
                }

                if (newStatus.ConferenceStatusName == "Cancelled")
                    await ValidateForCancelledStateAsync(conference);

                if (newStatus.ConferenceStatusName == "Cancelled")
                    await ValidateForCancelledStateAsync(conference);


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



        public async Task<DTOs.Conference.ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId, string? userId)
        {
            string ticketId = "", pricePhaseId = "", conferencePriceId = "";
            if (userId != null)
            {
                var ticket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(userId, conferenceId);
                ticketId = ticket?.TicketId;
                pricePhaseId = ticket?.PricePhaseId;
                conferencePriceId = ticket?.PricePhase?.ConferencePrice?.ConferencePriceId;
            }
            // Get the main conference with related data
            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                        .ThenInclude(pp => pp.RefundPolicies)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.ConferenceSessionMedia) // No speakers for research sessions
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room) // Include room information
                         .ThenInclude(r => r.Destination)
                            .ThenInclude(d => d.City)
                .Include(c => c.Sponsors)
                .Include(c => c.RefundPolicies)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference với ID {conferenceId} không tìm thấy");
            }

            if (conference.IsResearchConference == false)
                throw new Exception("chức năng chỉ dành cho research");

            // Get research conference detail if it exists (for research conferences)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);

            // Get research conference phase
            var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conferenceId);

            // Get related research data
            var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
            var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conferenceId);
            var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
            var researchSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);

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

                // Research Conference Detail specific fields
                Name = researchDetail?.Name,
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
                ResearchPhase = researchPhase != null ? researchPhase.Select(researchPhase => researchPhase.toResearchPhaseResponse()).ToList() : null,
                ResearchSessions = researchSessions?.Select(rs => rs.ToResearchSessionWithMediaResponse()).ToList(),

                Policies = conference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = conference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),

                ConferenceMedia = conference.ConferenceMedia?.Select(cm => cm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),
                purchasedInfo = new PurchasedInfo
                {
                    ticketId = ticketId,
                    conferencePriceId = conferencePriceId,
                    pricePhaseId = pricePhaseId
                }
            };
        }

        public async Task<DTOs.Conference.ResearchConferenceDetailResponse> GetDetailResearchForOrganizerAsync(string conferenceId)
        {
            // Get the main conference with related data and timeline
            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                        .ThenInclude(pp => pp.RefundPolicies)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.ConferenceSessionMedia) // No speakers for research sessions
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room) // Include room information
                         .ThenInclude(r => r.Destination)
                            .ThenInclude(d => d.City)
                .Include(c => c.Sponsors)
                .Include(c => c.RefundPolicies)
                .Include(c => c.ConferenceTimelines) // Include timeline
                    .ThenInclude(ct => ct.PreviousStatus)
                .Include(c => c.ConferenceTimelines)
                    .ThenInclude(ct => ct.AfterwardStatus)
                .Include(c => c.RefundPolicies)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference với ID {conferenceId} không tìm thấy");
            }

            if (conference.IsResearchConference == false)
                throw new Exception("chức năng chỉ dành cho research");

            // Get research conference detail if it exists (for research conferences)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);

            // Get research conference phase
            var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchPhaseByConfId(conferenceId);

            // Get related research data
            var rankingFileUrls = await _unitOfWork.RankingFileUrlRepository.GetRankingFileUrlsByConferenceIdAsync(conferenceId);
            var materialDownloads = await _unitOfWork.MaterialDownloadRepository.GetMaterialsByConferenceIdAsync(conferenceId);
            var rankingReferenceUrls = await _unitOfWork.RankingReferenceUrlRepository.GetRankingReferenceUrlsByConferenceIdAsync(conferenceId);
            var researchSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);

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

                // Research Conference Detail specific fields
                Name = researchDetail?.Name,
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
                ResearchPhase = researchPhase != null ? researchPhase.Select(researchPhase => researchPhase.toResearchPhaseResponse()).ToList() : null,
                ResearchSessions = researchSessions?.Select(rs => rs.ToResearchSessionWithMediaResponse()).ToList(),

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
            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // If the user is not an organizer, verify that they created the conference
            if (!isOrganizer)
            {
                if (conference.CreatedBy != userId)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có thể thấy detail cho conference bạn tạo ra.");
                }
            }

            // Now get the complete conference data with timeline
            var fullConference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                        .ThenInclude(pp => pp.RefundPolicies)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Speakers)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.ConferenceSessionMedia)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room) // Include room information
                         .ThenInclude(r => r.Destination)
                            .ThenInclude(d => d.City)
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                .Include(c => c.ConferenceTimelines) // Include timeline
                    .ThenInclude(ct => ct.PreviousStatus)
                .Include(c => c.ConferenceTimelines)
                    .ThenInclude(ct => ct.AfterwardStatus)
                .Include(c => c.RefundPolicies)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (fullConference == null)
            {
                throw new NotFoundException($"Conference với ID {conferenceId} không tìm thấy");
            }

            if (conference.IsResearchConference == true)
                throw new Exception("chức năng chỉ dành cho tech");

            // Get technical conference detail if it exists (for technical conferences)
            var technicalDetail = fullConference.TechnicalConferenceDetail;

            return new DTOs.Conference.TechnicalConferenceDetailResponse
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
                TargetAudience = technicalDetail?.TargetAudience, // Set to null if it's a research conference
                //commission = technicalDetail?.Commission,
                //contractURL = technicalDetail?.ContractUrl,
                createdBy = fullConference.CreatedBy,

                Policies = fullConference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                Sponsors = fullConference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                Sessions = fullConference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).ToList(),
                ConferenceMedia = fullConference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                ConferencePrices = fullConference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList(),

                // Include conference timeline data
                ConferenceTimelines = fullConference.ConferenceTimelines?.Select(ct => ct.ToConferenceTimelineResponse()).ToList(),

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
                    Name = researchDetail?.Name,
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
                    ResearchPhase = researchPhase != null ? researchPhase.Select(researchPhase => researchPhase.toResearchPhaseResponse()).ToList() : null,
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

        public async Task<PagedResult<DTOs.Conference.TechnicalConferenceDetailResponse>> GetTechnicalConferencesListAsync(
            int page, int pageSize, string? conferenceStatusId = null, string? searchKeyword = null,
            string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null,
            string? userId = null, bool isOrganizer = false)
        {
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            IQueryable<Conference> query;

            if (isOrganizer && !string.IsNullOrEmpty(conferenceStatusId) && conferenceStatusId == draftStatus.ConferenceStatusId)
            {
                throw new BadRequestException("Organizers không được phép xem chọn lọc theo trạng thái 'Draft'.");
            }
            if (isOrganizer)
            {
                // Organizers can see all technical conferences
                query = _unitOfWork.ConferenceRepository.GetAllConferences()
                    .Where(c => (c.IsResearchConference == false || c.IsResearchConference == null)
                    && c.ConferenceStatusId != draftStatus.ConferenceStatusId);
            }
            else
            {
                // Collaborators can only see technical conferences they created
                query = _unitOfWork.ConferenceRepository.GetAllConferences()
                    .Where(c => (c.IsResearchConference == false || c.IsResearchConference == null) && c.CreatedBy == userId);
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

            var responses = new List<DTOs.Conference.TechnicalConferenceDetailResponse>();

            foreach (var conference in pagedConferences)
            {
                // For each conference, get the detailed technical conference data
                var technicalDetail = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conference.ConferenceId);

                var responsesList = await _unitOfWork.ConferenceRepository.GetAllConferences()
                    .Include(c => c.ConferenceCategory)
                    .Include(c => c.ConferenceMedia)
                    .Include(c => c.Policies)
                    .Include(c => c.ConferencePrices)
                        .ThenInclude(cp => cp.PricePhases)
                            .ThenInclude(pp => pp.RefundPolicies)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.Speakers)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.ConferenceSessionMedia)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.Room) // Include room information
                            .ThenInclude(r => r.Destination)
                                .ThenInclude(d => d.City)
                    .Include(c => c.Sponsors)
                    .Include(c => c.TechnicalConferenceDetail)
                    .Where(c => c.ConferenceId == conference.ConferenceId)
                    .ToListAsync();

                var fullConference = responsesList.FirstOrDefault();

                if (fullConference != null)
                {
                    var response = new DTOs.Conference.TechnicalConferenceDetailResponse
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
                        TargetAudience = technicalDetail?.TargetAudience, // Set to null if it's a research conference
                        //contractURL = technicalDetail?.ContractUrl,
                        //commission = technicalDetail?.Commission,
                        createdBy = fullConference.CreatedBy,
                        Policies = fullConference.Policies?.Select(p => p.ToConferencePolicyResponse()).ToList(),
                        Sponsors = fullConference.Sponsors?.Select(s => s.ToSponsorResponse()).ToList(),
                        Sessions = fullConference.ConferenceSessions?.Select(cs => cs.ToConferenceSessionWithSpeakersResponse()).ToList(),
                        ConferenceMedia = fullConference.ConferenceMedia?.Select(cfm => cfm.ToConferenceMediaResponse()).ToList(),
                        ConferencePrices = fullConference.ConferencePrices?.Select(cp => cp.ToConferencePriceWithPhasesResponse()).ToList()
                    };

                    responses.Add(response);
                }
            }

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
            var userCheckInFound = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(request.ConferenceSessionId, userId);
            if (userCheckInFound == null)
            {
                throw new BadRequestException($"Bạn chưa mua vé nào nên không thể dánh giá");
            }
            var pendingCheckInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            if (pendingCheckInStatus == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái check in trong hệ thống");
            }
            if (userCheckInFound.CheckinStatusId == pendingCheckInStatus.CheckinStatusId)
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
            if (readyStatusConference == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái ready cho hội nghị");
            }
            return await _unitOfWork.ConferenceRepository.GetListConferencesForScheduleByUserId(userId, await _timeProviderService.GetVietnamDate(), readyStatusConference.ConferenceStatusId);
        }

        public async Task<List<ConferenceResponseDTO>> GetConferenceByAssignedPapers(string? userId)
        {
            List<PaperReviewer> AssignPaper = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAsync(userId);
            List<Conference> AssignedConference = AssignPaper.Select(ap => ap.Paper.Conference).OrderByDescending(c => c.CreatedAt).ToList();
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
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conference == null) throw new BadRequestException($"Không tìm thấy hội nghị với ID: {confId}");
            if (conference.CreatedBy != userId) throw new BadRequestException("Bạn không có quyền gởi yêu cầu  approve cho hội nghị này");
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            if (conference.ConferenceStatusId != draftStatus.ConferenceStatusId) throw new BadRequestException($" conference với ID {confId} phải dang là draft status mới có thể yêu cầu duyệt được");
            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId) throw new BadRequestException("Hội nghị đã gửi yêu cầu được duyệt trước đó rồi xin chờ kết quả!");
            return await UpdateConferenceStatusAsync(confId, pendingStatus.ConferenceStatusName, $"Collborator với ID: {userId} dang request conference với ID: {confId} để được duyệt");
        }

        // DÁN TOÀN B? PHIÊN B?N NÀY Ð? THAY TH? PHIÊN B?N CU

        public async Task<bool> ActivateWaitlist(string confId, string userId)
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
            var notWaitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseNotWaitListByConferenceIdAsync(confId);
            var waitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(confId);
            if (notWaitlistPhase == null || waitlistPhase == null)
                throw new BadRequestException("Hội nghị chưa được cấu hình đầy đủ phase chính và phase waitlist.");

            // 1.3. L?y Research Detail (c?n cho các bu?c sau)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(confId);
            if (researchDetail == null)
                throw new InvalidOperationException($"Hội nghị chưa có chi tiết nghiên cứu (Research Detail).");

            // 1.4. Ki?m tra xem waitlist dã du?c kích ho?t chua
            if (waitlistPhase.IsActive == true) // Ch? c?n ki?m tra phase waitlist là d?
                throw new BadRequestException($"Waitlist cho hội nghị này đã kích hoạt truớc đó.");





            #region === 2. VALIDATION LOGIC NGHI?P V? ===

            // 2.1. Ki?m tra s? lu?ng vé Author còn l?i
            var authorConferencePrices = await _unitOfWork.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(confId);
            var remainingAuthorSlots = authorConferencePrices.Sum(cp => cp.AvailableSlot ?? 0);
            if (remainingAuthorSlots <= 0)
                throw new BadRequestException("Không theer kích ho?t waitlist vì t?t c? các su?t dành cho tác gi? (vé 'isAuthor') dã du?c bán h?t.");

            // 2.2. Ki?m tra di?u ki?n th?i gian
            var today = await _timeProviderService.GetVietnamDate();
            // 2.2a. Ph?i sau khi phase chính k?t thúc hoàn toàn (k?t thúc Camera Ready)
            if (today <= notWaitlistPhase.CameraReadyEndDate)
                throw new BadRequestException($"Không th? kích ho?t waitlist khi phase chính chua k?t thúc. Phase chính k?t thúc vào ngày: {notWaitlistPhase.CameraReadyEndDate:dd/MM/yyyy}.");

            // 2.2b. Ph?i n?m trong kho?ng th?i gian dang ký c?a phase waitlist
            if (today < waitlistPhase.RegistrationStartDate || today > waitlistPhase.RegistrationEndDate)
                throw new BadRequestException($"Ch? có th? kích ho?t waitlist trong kho?ng th?i gian dang ký c?a nó ({waitlistPhase.RegistrationStartDate:dd/MM/yyyy} - {waitlistPhase.RegistrationEndDate:dd/MM/yyyy}).");

            // 2.3. Ki?m tra xem ngu?i t? ch?c dã t?o PricePhase cho vé Author trong giai do?n Waitlist chua
            var allAuthorPricePhases = await _unitOfWork.PricePhaseRepository.GetPricePhaseByconferenceIdThatIsAuthor(confId);
            bool hasPricePhaseForWaitlist = allAuthorPricePhases.Any(pp => pp.ResearchConferencePhaseId == waitlistPhase.ResearchConferencePhaseId);

            if (!hasPricePhaseForWaitlist)
                throw new BadRequestException("Không th? kích ho?t waitlist. Vui lòng t?o ít nh?t m?t 'Giai do?n bán vé' (Price Phase) cho lo?i vé 'isAuthor' có kho?ng th?i gian n?m trong giai do?n dang ký c?a waitlist.");

            #endregion

            #region === 3. TH?C THI THAY Ð?I ===
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                waitlistPhase.IsActive = true;
                notWaitlistPhase.IsActive = false;

                await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(waitlistPhase);
                await _unitOfWork.ResearchConferencePhaseRepository.UpdateResearchConferencePhaseAsync(notWaitlistPhase);

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

        public async Task OnholdToReadyValidAsync(Conference conf, string readyId, string onHoldId)
        {
            var onHoldStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.OnHold.GetDescription());
            var onHoldTimelineEntry = await _unitOfWork.ConferenceTimelineRepository.GetLastOnHoldConferenceTimelineByConfIdAndStatusIdAsync(conf.ConferenceId, readyId, onHoldId);
            if (onHoldTimelineEntry == null)
                throw new InvalidOperationException("Không tìm thấy lịch sử chuyển sang trạng thái 'OnHold'.");

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


        public async Task ValidateForReadyStateAsync(Conference conf)
        {
            var invalidMessages = new List<string>();

            // --- BƯỚC A: KIỂM TRA SỰ ĐẦY ĐỦ THÔNG TIN ---
            // Đây là phần validation riêng của trạng thái Ready

            var price = await _unitOfWork.ConferencePriceRepository.AnyConferencePriceWithAtLeastOnePricePhase(conf.ConferenceId);
            if (price == null)
                invalidMessages.Add("Hội nghị phải có ít nhất một loại vé, trong đó có ít nhất một phase.");
            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conf.ConferenceId);
            if (!sessions.Any())
                invalidMessages.Add("Hội nghị phải có ít nhất một phiên.");

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

            // Kiểm tra tất cả hội nghị phải có ít nhất một chính sách
            var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conf.ConferenceId);
            if (!policies.Any())
            {
                invalidMessages.Add("Hội nghị phải có ít nhất một chính sách.");
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
            var timelineErrors = await ValidateConferenceTimelineAsync(conf, dateOnlyRule);
            invalidMessages.AddRange(timelineErrors); // Thêm các lỗi timeline vào danh sách chung

            if (invalidMessages.Any())
            {
                string errorMessage = "Không thể chuyển sang trạng thái 'Ready'. Vui lòng khắc phục các vấn đề sau:\n- "
                                    + string.Join("|", invalidMessages.Distinct());
                throw new BadRequestException(errorMessage);
            }
        }
    }
}