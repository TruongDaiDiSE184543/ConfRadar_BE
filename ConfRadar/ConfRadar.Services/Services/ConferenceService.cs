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
        Task<PagedResult<ConferenceResponse>> GetAllConferencesPaginatedAsync(int page, int pageSize);

        // NEW ENDPOINTS
        // Endpoint 1: Get all conferences with their price phases (with pagination/filtering)
        Task<PagedResult<ConferenceWithPricesResponse>> GetConferencesWithPricesAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // Endpoint 2: Get detailed technical conference data
        Task<TechnicalConferenceDetailResponse> GetTechnicalConferenceDetailAsync(string conferenceId, string? userId);

        // Endpoint 3: Get conferences by status ID with filtering
        Task<PagedResult<ConferenceResponse>> GetConferencesByStatusAsync(string conferenceStatusId, int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // Endpoint 4: Get conferences with step completion status
        Task<PagedResult<ConferenceStepCompletionStatusResponse>> GetTechnicalConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // NEW ENDPOINT 5: Get all pending conferences
        Task<PagedResult<ConferenceResponse>> GetPendingConferencesAsync(int page, int pageSize, string? searchKeyword = null);

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
        Task<List<ConferenceResponse>> GetConferenceByAssignedPapers(string? userId);
        Task<bool> RequestOrganizerApproval(string confId, string userId);
        Task<bool> ActivateWaitlist(string confId, string userId);
    }

    public class ConferenceService : IConferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConferenceStatusService _conferenceStatusService;
        private readonly IConferenceTimelineService _conferenceTimelineService;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly ISystemConfigurationService _systemConfigurationService;

        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceService(IUnitOfWork unitOfWork, IConferenceStatusService conferenceStatusService, IConferenceTimelineService conferenceTimelineService, IObjectStorageFileService objectStorageFileService, ITokenService tokenService, ISystemConfigurationService systemConfigurationService, IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _conferenceStatusService = conferenceStatusService;
            _conferenceTimelineService = conferenceTimelineService;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _systemConfigurationService = systemConfigurationService;
            _objectStorageSettings = objectStorageSettings.Value;
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


        #endregion


        public async Task<PagedResult<ConferenceResponse>> GetAllConferencesPaginatedAsync(int page, int pageSize)
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
            var responses = pagedConferences.Select(conference => new ConferenceResponse
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
            return new PagedResult<ConferenceResponse>
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
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
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
                TicketSaleEnd = conference.TicketSaleEnd,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                ConferenceStatusId = conference.ConferenceStatusId,
                TargetAudience = technicalDetail?.TargetAudience, // Set to null if it's a research conference
                contractURL = technicalDetail.ContractUrl,
                commission = technicalDetail.Commission,
                //RefundPolicies = conference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                //{
                //    RefundPolicyId = rp.RefundPolicyId,
                //    PercentRefund = rp.PercentRefund,
                //    RefundDeadline = rp.RefundDeadline,
                //    RefundOrder = rp.RefundOrder
                //}).ToList(),
                Policies = conference.Policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl
                }).ToList(),
                Sessions = conference.ConferenceSessions?.Select(cs => new ConferenceSessionWithSpeakersResponse
                {
                    ConferenceSessionId = cs.ConferenceSessionId,
                    Title = cs.Title,
                    Description = cs.Description,
                    StartTime = cs.StartTime,
                    EndTime = cs.EndTime,
                    SessionDate = cs.SessionDate,
                    ConferenceId = cs.ConferenceId,
                    RoomId = cs.RoomId,
                    Room = cs.Room != null ? new DTOs.Conference.RoomInfoResponse // Include room information
                    {
                        RoomId = cs.Room.RoomId,
                        Number = cs.Room.Number,
                        DisplayName = cs.Room.DisplayName,
                        DestinationId = cs.Room.DestinationId
                    } : null,
                    Speakers = cs.Speakers?.Select(s => new DTOs.Conference.SpeakerResponse
                    {
                        SpeakerId = s.SpeakerId,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image
                    }).ToList(),
                    SessionMedia = cs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                    {
                        ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                        ConferenceSessionMediaUrl = csm.MediaUrl
                    }).ToList()
                }).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cfm => new DTOs.Conference.ConferenceMediaResponse
                {
                    MediaId = cfm.ConferenceMediaId,
                    MediaUrl = cfm.ConferenceMediaUrl
                }).ToList(),
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
                        RefundPolicies = pp.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        {
                            RefundPolicyId = rp.RefundPolicyId,
                            PercentRefund = rp.PercentRefund,
                            RefundDeadline = rp.RefundDeadline,
                            RefundOrder = rp.RefundOrder,
                            PricePhaseID = pp.PricePhaseId
                        }).OrderBy(rp => rp.RefundOrder).ToList(),
                    }).ToList()
                }).ToList(),
                purchasedInfo = new PurchasedInfo
                {
                    ticketId = ticketId,
                    conferencePriceId = conferencePriceId,
                    pricePhaseId = pricePhaseId
                }
            };
        }

        public async Task<PagedResult<ConferenceResponse>> GetConferencesByStatusAsync(string conferenceStatusId, int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
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

            var responses = pagedConferences.Select(conference => new ConferenceResponse
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

            return new PagedResult<ConferenceResponse>
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

        public async Task<PagedResult<ConferenceResponse>> GetPendingConferencesAsync(int page, int pageSize, string? searchKeyword = null)
        {
            // Get the "Pending" status ID first
            var allStatuses = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
            var pendingStatus = allStatuses.FirstOrDefault(s => s.ConferenceStatusName == "Pending");

            if (pendingStatus == null)
            {
                return new PagedResult<ConferenceResponse>
                {
                    Items = new List<ConferenceResponse>(),
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

            var responses = pagedConferences.Select(conference => new ConferenceResponse
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

            return new PagedResult<ConferenceResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ApproveConferenceAsync(string conferenceId, ApproveConferenceRequest request)
        {
            // Change conference status from Pending to Rejected
            if (request.IsApprove == false) return await UpdateConferenceStatusAsync(conferenceId, "Rejected", request.Reason);
            // Change conference status from Pending to Preparing
            return await UpdateConferenceStatusAsync(conferenceId, "Preparing", request.Reason);
        }

        public async Task<bool> ChangeConferenceStatus(string userId, string conferenceId, string newStatus, string? reason = null)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new BadRequestException($"Không tìm thấy hội nghị với id {conferenceId}");
            if (conference.CreatedBy != userId) throw new BadRequestException("Chỉ có người tạo ra conference mới thay đổi được trạng thái");

            //Collaborator's technical confs need to be approved to preparing status first only then they can change the status
            var newStatusEntity = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(newStatus);
            if (newStatusEntity == null) throw new BadRequestException($"Không tim thấy conference status với ID {newStatus}");

            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            var deleteStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Deleted.GetDescription());
            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId) throw new Exception("Conference cần Organizer approve lên preparing first để có thể thay đổi trạng thái");
            if (conference.ConferenceStatusId == draftStatus.ConferenceStatusId && newStatusEntity.ConferenceStatusId != deleteStatus.ConferenceStatusId) throw new Exception("Hiện tại bản draft của conference chỉ có thể chuyển sang delete.Conference cần request lên pending để Organizer approve lên preparing first để có thể thay đổi trạng thái khác");
            

            return UpdateConferenceStatusAsync(conferenceId, newStatusEntity.ConferenceStatusName!, reason).Result;
        }

        public async Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new BadRequestException("Không tìm thấy conf id này");
            }

            // Get current status name from the conference status ID
            var currentStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByIdAsync(conference.ConferenceStatusId);
            if (currentStatus == null)
            {
                throw new BadRequestException("Không tìm thấy trạng thái hiện tại của hội nghị");
            }

            // Get the new status by name
            var newStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(newStatusName);
            if (newStatus == null)
            {
                throw new BadRequestException($"Không tồn tại trạng thái {newStatus}");
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

                // Update the conference status
                conference.ConferenceStatusId = newStatus.ConferenceStatusId;

                // Create a timeline record for the status change
                var timelineRecord = new CreateConferenceTimelineRequest
                {
                    ConferenceId = conferenceId,
                    ChangeDate = ExtensionHelper.GetVietnamDate(),
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
                .Include(c => c.Sponsors)
                .Include(c => c.RefundPolicies)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // Get research conference detail if it exists (for research conferences)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);

            // Get research conference phase
            var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conferenceId);

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
                RankingFileUrls = rankingFileUrls?.Select(r => new DTOs.Conference.RankingFileUrlResponse
                {
                    RankingFileUrlId = r.RankingFileUrlId,
                    FileUrl = r.FileUrl
                }).ToList(),
                MaterialDownloads = materialDownloads?.Select(m => new DTOs.Conference.MaterialDownloadResponse
                {
                    MaterialDownloadId = m.MaterialDownloadId,
                    FileName = m.FileName,
                    FileDescription = m.FileDescription,
                    FileUrl = m.FileName
                }).ToList(),
                RankingReferenceUrls = rankingReferenceUrls?.Select(r => new DTOs.Conference.RankingReferenceUrlResponse
                {
                    ReferenceUrlId = r.ReferenceUrlId,
                    ReferenceUrl = r.ReferenceUrl
                }).ToList(),
                ResearchPhase = researchPhase != null ? new DTOs.Conference.ResearchConferencePhaseResponse
                {
                    ResearchConferencePhaseId = researchPhase.ResearchConferencePhaseId,
                    ConferenceId = researchPhase.ConferenceId,
                    RegistrationStartDate = researchPhase.RegistrationStartDate,
                    RegistrationEndDate = researchPhase.RegistrationEndDate,
                    FullPaperStartDate = researchPhase.FullPaperStartDate,
                    FullPaperEndDate = researchPhase.FullPaperEndDate,
                    ReviewStartDate = researchPhase.ReviewStartDate,
                    ReviewEndDate = researchPhase.ReviewEndDate,
                    ReviseStartDate = researchPhase.ReviseStartDate,
                    ReviseEndDate = researchPhase.ReviseEndDate,
                    CameraReadyStartDate = researchPhase.CameraReadyStartDate,
                    CameraReadyEndDate = researchPhase.CameraReadyEndDate,
                    IsWaitlist = researchPhase.IsWaitlist,
                    IsActive = researchPhase.IsActive,
                    RevisionRoundDeadlines = researchPhase.RevisionRoundDeadlines?.Select(r => new DTOs.Conference.RevisionRoundDeadlineResponse
                    {
                        RevisionRoundDeadlineId = r.RevisionRoundDeadlineId,
                        EndDate = r.EndSubmissionDate,
                        RoundNumber = r.RoundNumber,
                        ResearchConferencePhaseId = r.ResearchConferencePhaseId
                    }).ToList()
                } : null,
                ResearchSessions = researchSessions?.Select(rs => new DTOs.Conference.ResearchSessionWithMediaResponse
                {
                    ConferenceSessionId = rs.ConferenceSessionId,
                    Title = rs.Title,
                    Description = rs.Description,
                    StartTime = rs.StartTime.HasValue ? TimeOnly.FromDateTime(rs.StartTime.Value) : null,
                    EndTime = rs.EndTime.HasValue ? TimeOnly.FromDateTime(rs.EndTime.Value) : null,
                    Date = rs.SessionDate,
                    ConferenceId = rs.ConferenceId,
                    RoomId = rs.RoomId,
                    Room = rs.Room != null ? new DTOs.Conference.RoomInfoResponse // Include room information for research sessions
                    {
                        RoomId = rs.Room.RoomId,
                        Number = rs.Room.Number,
                        DisplayName = rs.Room.DisplayName,
                        DestinationId = rs.Room.DestinationId
                    } : null,
                    // Note: No speakers for research sessions
                    SessionMedia = rs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                    {
                        ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                        ConferenceSessionMediaUrl = csm.MediaUrl
                    }).ToList()
                }).ToList(),

                // Shared tables data (same as technical conference)
                Policies = conference.Policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl
                }).ToList(),
                //RefundPolicies = conference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                //{
                //    RefundPolicyId = rp.RefundPolicyId,
                //    PercentRefund = rp.PercentRefund,
                //    RefundDeadline = rp.RefundDeadline,
                //    RefundOrder = rp.RefundOrder
                //}).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cm => new DTOs.Conference.ConferenceMediaResponse
                {
                    MediaId = cm.ConferenceMediaId,
                    MediaUrl = cm.ConferenceMediaUrl
                }).ToList(),
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
                        RefundPolicies = pp.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        {
                            RefundPolicyId = rp.RefundPolicyId,
                            PercentRefund = rp.PercentRefund,
                            RefundDeadline = rp.RefundDeadline,
                            RefundOrder = rp.RefundOrder
                        }).OrderBy(rp => rp.RefundOrder).ToList(),
                    }).ToList()
                }).ToList(),
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
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // Get research conference detail if it exists (for research conferences)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(conferenceId);

            // Get research conference phase
            var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conferenceId);

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
                RankingFileUrls = rankingFileUrls?.Select(r => new DTOs.Conference.RankingFileUrlResponse
                {
                    RankingFileUrlId = r.RankingFileUrlId,
                    FileUrl = r.FileUrl
                }).ToList(),
                MaterialDownloads = materialDownloads?.Select(m => new DTOs.Conference.MaterialDownloadResponse
                {
                    MaterialDownloadId = m.MaterialDownloadId,
                    FileName = m.FileName,
                    FileDescription = m.FileDescription,
                    FileUrl = m.FileName
                }).ToList(),
                RankingReferenceUrls = rankingReferenceUrls?.Select(r => new DTOs.Conference.RankingReferenceUrlResponse
                {
                    ReferenceUrlId = r.ReferenceUrlId,
                    ReferenceUrl = r.ReferenceUrl
                }).ToList(),
                ResearchPhase = researchPhase != null ? new DTOs.Conference.ResearchConferencePhaseResponse
                {
                    ResearchConferencePhaseId = researchPhase.ResearchConferencePhaseId,
                    ConferenceId = researchPhase.ConferenceId,
                    RegistrationStartDate = researchPhase.RegistrationStartDate,
                    RegistrationEndDate = researchPhase.RegistrationEndDate,
                    FullPaperStartDate = researchPhase.FullPaperStartDate,
                    FullPaperEndDate = researchPhase.FullPaperEndDate,
                    ReviewStartDate = researchPhase.ReviewStartDate,
                    ReviewEndDate = researchPhase.ReviewEndDate,
                    ReviseStartDate = researchPhase.ReviseStartDate,
                    ReviseEndDate = researchPhase.ReviseEndDate,
                    CameraReadyStartDate = researchPhase.CameraReadyStartDate,
                    CameraReadyEndDate = researchPhase.CameraReadyEndDate,
                    IsWaitlist = researchPhase.IsWaitlist,
                    IsActive = researchPhase.IsActive,
                    RevisionRoundDeadlines = researchPhase.RevisionRoundDeadlines?.Select(r => new DTOs.Conference.RevisionRoundDeadlineResponse
                    {
                        RevisionRoundDeadlineId = r.RevisionRoundDeadlineId,
                        //EndDate = r.EndDate,
                        RoundNumber = r.RoundNumber,
                        ResearchConferencePhaseId = r.ResearchConferencePhaseId
                    }).ToList()
                } : null,
                ResearchSessions = researchSessions?.Select(rs => new DTOs.Conference.ResearchSessionWithMediaResponse
                {
                    ConferenceSessionId = rs.ConferenceSessionId,
                    Title = rs.Title,
                    Description = rs.Description,
                    StartTime = rs.StartTime.HasValue ? TimeOnly.FromDateTime(rs.StartTime.Value) : null,
                    EndTime = rs.EndTime.HasValue ? TimeOnly.FromDateTime(rs.EndTime.Value) : null,
                    Date = rs.SessionDate,
                    ConferenceId = rs.ConferenceId,
                    RoomId = rs.RoomId,
                    Room = rs.Room != null ? new DTOs.Conference.RoomInfoResponse // Include room information for research sessions
                    {
                        RoomId = rs.Room.RoomId,
                        Number = rs.Room.Number,
                        DisplayName = rs.Room.DisplayName,
                        DestinationId = rs.Room.DestinationId
                    } : null,
                    // Note: No speakers for research sessions
                    SessionMedia = rs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                    {
                        ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                        ConferenceSessionMediaUrl = csm.MediaUrl
                    }).ToList()
                }).ToList(),

                // Shared tables data (same as technical conference)
                Policies = conference.Policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl
                }).ToList(),
                //RefundPolicies = conference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                //{
                //    RefundPolicyId = rp.RefundPolicyId,
                //    PercentRefund = rp.PercentRefund,
                //    RefundDeadline = rp.RefundDeadline,
                //    RefundOrder = rp.RefundOrder
                //}).ToList(),
                ConferenceMedia = conference.ConferenceMedia?.Select(cm => new DTOs.Conference.ConferenceMediaResponse
                {
                    MediaId = cm.ConferenceMediaId,
                    MediaUrl = cm.ConferenceMediaUrl
                }).ToList(),
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
                        RefundPolicies = pp.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        {
                            RefundPolicyId = rp.RefundPolicyId,
                            PercentRefund = rp.PercentRefund,
                            RefundDeadline = rp.RefundDeadline,
                            RefundOrder = rp.RefundOrder,
                            PricePhaseID = pp.PricePhaseId
                        }).OrderBy(rp => rp.RefundOrder).ToList(),
                    }).ToList(),

                }).ToList(),

                // Include conference timeline data
                ConferenceTimelines = conference.ConferenceTimelines?.Select(ct => new DTOs.Conference.ConferenceTimelineResponse
                {
                    ConferenceTimelineId = ct.ConferenceTimelineId,
                    ConferenceId = ct.ConferenceId,
                    ChangeDate = ct.ChangeDate,
                    PreviousStatusId = ct.PreviousStatusId,
                    AfterwardStatusId = ct.AfterwardStatusId,
                    Reason = ct.Reason,
                    PreviousStatusName = ct.PreviousStatus?.ConferenceStatusName,
                    AfterwardStatusName = ct.AfterwardStatus?.ConferenceStatusName,
                    ConferenceName = ct.Conference?.ConferenceName
                }).ToList()
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
                    throw new UnauthorizedAccessException("You are not authorized to view this conference detail.");
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
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

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
                commission = technicalDetail?.Commission,
                contractURL = technicalDetail?.ContractUrl,
                //RefundPolicies = fullConference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                //{
                //    RefundPolicyId = rp.RefundPolicyId,
                //    PercentRefund = rp.PercentRefund,
                //    RefundDeadline = rp.RefundDeadline,
                //    RefundOrder = rp.RefundOrder
                //}).ToList(),
                Policies = fullConference.Policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Sponsors = fullConference.Sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl
                }).ToList(),
                Sessions = fullConference.ConferenceSessions?.Select(cs => new DTOs.Conference.ConferenceSessionWithSpeakersResponse
                {
                    ConferenceSessionId = cs.ConferenceSessionId,
                    Title = cs.Title,
                    Description = cs.Description,
                    StartTime = cs.StartTime,
                    EndTime = cs.EndTime,
                    SessionDate = cs.SessionDate,
                    ConferenceId = cs.ConferenceId,
                    RoomId = cs.RoomId,
                    Room = cs.Room != null ? new DTOs.Conference.RoomInfoResponse // Include room information
                    {
                        RoomId = cs.Room.RoomId,
                        Number = cs.Room.Number,
                        DisplayName = cs.Room.DisplayName,
                        DestinationId = cs.Room.DestinationId
                    } : null,
                    Speakers = cs.Speakers?.Select(s => new DTOs.Conference.SpeakerResponse
                    {
                        SpeakerId = s.SpeakerId,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image
                    }).ToList(),
                    SessionMedia = cs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                    {
                        ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                        ConferenceSessionMediaUrl = csm.MediaUrl
                    }).ToList()
                }).ToList(),
                ConferenceMedia = fullConference.ConferenceMedia?.Select(cfm => new DTOs.Conference.ConferenceMediaResponse
                {
                    MediaId = cfm.ConferenceMediaId,
                    MediaUrl = cfm.ConferenceMediaUrl
                }).ToList(),
                ConferencePrices = fullConference.ConferencePrices?.Select(cp => new DTOs.Conference.ConferencePriceWithPhasesResponse
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
                        RefundPolicies = pp.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        {
                            RefundPolicyId = rp.RefundPolicyId,
                            PercentRefund = rp.PercentRefund,
                            RefundDeadline = rp.RefundDeadline,
                            RefundOrder = rp.RefundOrder,
                            PricePhaseID = pp.PricePhaseId
                        }).OrderBy(rp => rp.RefundOrder).ToList(),
                    }).ToList()
                }).ToList(),

                // Include conference timeline data
                ConferenceTimelines = fullConference.ConferenceTimelines?.Select(ct => new DTOs.Conference.ConferenceTimelineResponse
                {
                    ConferenceTimelineId = ct.ConferenceTimelineId,
                    ConferenceId = ct.ConferenceId,
                    ChangeDate = ct.ChangeDate,
                    PreviousStatusId = ct.PreviousStatusId,
                    AfterwardStatusId = ct.AfterwardStatusId,
                    Reason = ct.Reason,
                    PreviousStatusName = ct.PreviousStatus?.ConferenceStatusName,
                    AfterwardStatusName = ct.AfterwardStatus?.ConferenceStatusName,
                    ConferenceName = ct.Conference?.ConferenceName
                }).ToList(),

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
                    .Where(c => c.IsResearchConference == true).OrderByDescending(c => c.CreatedAt);
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
                var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetActiveResearchConferencePhaseByConferenceIdAsync(conference.ConferenceId);
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
                    RankingFileUrls = rankingFileUrls?.Select(r => new DTOs.Conference.RankingFileUrlResponse
                    {
                        RankingFileUrlId = r.RankingFileUrlId,
                        FileUrl = r.FileUrl
                    }).ToList(),
                    MaterialDownloads = materialDownloads?.Select(m => new DTOs.Conference.MaterialDownloadResponse
                    {
                        MaterialDownloadId = m.MaterialDownloadId,
                        FileName = m.FileName,
                        FileDescription = m.FileDescription,
                        FileUrl = m.FileName
                    }).ToList(),
                    RankingReferenceUrls = rankingReferenceUrls?.Select(r => new DTOs.Conference.RankingReferenceUrlResponse
                    {
                        ReferenceUrlId = r.ReferenceUrlId,
                        ReferenceUrl = r.ReferenceUrl
                    }).ToList(),
                    ResearchPhase = researchPhase != null ? new DTOs.Conference.ResearchConferencePhaseResponse
                    {
                        ResearchConferencePhaseId = researchPhase.ResearchConferencePhaseId,
                        ConferenceId = researchPhase.ConferenceId,
                        RegistrationStartDate = researchPhase.RegistrationStartDate,
                        RegistrationEndDate = researchPhase.RegistrationEndDate,
                        FullPaperStartDate = researchPhase.FullPaperStartDate,
                        FullPaperEndDate = researchPhase.FullPaperEndDate,
                        ReviewStartDate = researchPhase.ReviewStartDate,
                        ReviewEndDate = researchPhase.ReviewEndDate,
                        ReviseStartDate = researchPhase.ReviseStartDate,
                        ReviseEndDate = researchPhase.ReviseEndDate,
                        CameraReadyStartDate = researchPhase.CameraReadyStartDate,
                        CameraReadyEndDate = researchPhase.CameraReadyEndDate,
                        IsWaitlist = researchPhase.IsWaitlist,
                        IsActive = researchPhase.IsActive,
                        RevisionRoundDeadlines = researchPhase.RevisionRoundDeadlines?.Select(r => new DTOs.Conference.RevisionRoundDeadlineResponse
                        {
                            RevisionRoundDeadlineId = r.RevisionRoundDeadlineId,
                            EndDate = r.EndSubmissionDate,
                            RoundNumber = r.RoundNumber,
                            ResearchConferencePhaseId = r.ResearchConferencePhaseId
                        }).ToList()
                    } : null,
                    ResearchSessions = researchSessions?.Select(rs => new DTOs.Conference.ResearchSessionWithMediaResponse
                    {
                        ConferenceSessionId = rs.ConferenceSessionId,
                        Title = rs.Title,
                        Description = rs.Description,
                        StartTime = rs.StartTime.HasValue ? TimeOnly.FromDateTime(rs.StartTime.Value) : null,
                        EndTime = rs.EndTime.HasValue ? TimeOnly.FromDateTime(rs.EndTime.Value) : null,
                        Date = rs.SessionDate,
                        ConferenceId = rs.ConferenceId,
                        RoomId = rs.RoomId,
                        Room = rs.Room != null ? new DTOs.Conference.RoomInfoResponse
                        {
                            RoomId = rs.Room.RoomId,
                            Number = rs.Room.Number,
                            DisplayName = rs.Room.DisplayName,
                            DestinationId = rs.Room.DestinationId
                        } : null,
                        SessionMedia = rs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                        {
                            ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                            ConferenceSessionMediaUrl = csm.MediaUrl
                        }).ToList()
                    }).ToList(),

                    // Shared tables data (same as technical conference)
                    Policies = policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                    {
                        PolicyId = p.PolicyId,
                        PolicyName = p.PolicyName,
                        Description = p.Description
                    }).ToList(),
                    Sponsors = sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                    {
                        SponsorId = s.SponsorId,
                        Name = s.Name,
                        ImageUrl = s.ImageUrl
                    }).ToList(),
                    //RefundPolicies = refundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                    //{
                    //    RefundPolicyId = rp.RefundPolicyId,
                    //    PercentRefund = rp.PercentRefund,
                    //    RefundDeadline = rp.RefundDeadline,
                    //    RefundOrder = rp.RefundOrder
                    //}).ToList(),
                    ConferenceMedia = conferenceMedia?.Select(cm => new DTOs.Conference.ConferenceMediaResponse
                    {
                        MediaId = cm.ConferenceMediaId,
                        MediaUrl = cm.ConferenceMediaUrl
                    }).ToList(),
                    ConferencePrices = conferencePrices?.Select(cp => new DTOs.Conference.ConferencePriceWithPhasesResponse
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
                            RefundPolicies = refundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                            {
                                RefundPolicyId = rp.RefundPolicyId,
                                PercentRefund = rp.PercentRefund,
                                RefundDeadline = rp.RefundDeadline,
                                RefundOrder = rp.RefundOrder,
                                PricePhaseID = pp.PricePhaseId
                            }).ToList(),
                        }).ToList()
                    }).ToList()
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

            if(isOrganizer && !string.IsNullOrEmpty(conferenceStatusId)  && conferenceStatusId == draftStatus.ConferenceStatusId)
            {
                throw new BadRequestException("Organizers không được phép xem hoặc lọc theo trạng thái 'Draft'.");
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
                        contractURL = technicalDetail?.ContractUrl,
                        commission = technicalDetail?.Commission,
                        //RefundPolicies = fullConference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                        //{
                        //    RefundPolicyId = rp.RefundPolicyId,
                        //    PercentRefund = rp.PercentRefund,
                        //    RefundDeadline = rp.RefundDeadline,
                        //    RefundOrder = rp.RefundOrder
                        //}).ToList(),
                        Policies = fullConference.Policies?.Select(p => new DTOs.Conference.ConferencePolicyResponse
                        {
                            PolicyId = p.PolicyId,
                            PolicyName = p.PolicyName,
                            Description = p.Description
                        }).ToList(),
                        Sponsors = fullConference.Sponsors?.Select(s => new DTOs.Conference.SponsorResponse
                        {
                            SponsorId = s.SponsorId,
                            Name = s.Name,
                            ImageUrl = s.ImageUrl
                        }).ToList(),
                        Sessions = fullConference.ConferenceSessions?.Select(cs => new DTOs.Conference.ConferenceSessionWithSpeakersResponse
                        {
                            ConferenceSessionId = cs.ConferenceSessionId,
                            Title = cs.Title,
                            Description = cs.Description,
                            StartTime = cs.StartTime,
                            EndTime = cs.EndTime,
                            SessionDate = cs.SessionDate,
                            ConferenceId = cs.ConferenceId,
                            RoomId = cs.RoomId,
                            Room = cs.Room != null ? new DTOs.Conference.RoomInfoResponse // Include room information
                            {
                                RoomId = cs.Room.RoomId,
                                Number = cs.Room.Number,
                                DisplayName = cs.Room.DisplayName,
                                DestinationId = cs.Room.DestinationId
                            } : null,
                            Speakers = cs.Speakers?.Select(s => new DTOs.Conference.SpeakerResponse
                            {
                                SpeakerId = s.SpeakerId,
                                Name = s.Name,
                                Description = s.Description,
                                Image = s.Image
                            }).ToList(),
                            SessionMedia = cs.ConferenceSessionMedia?.Select(csm => new DTOs.Conference.ConferenceSessionMediaResponse
                            {
                                ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                                ConferenceSessionMediaUrl = csm.MediaUrl
                            }).ToList()
                        }).ToList(),
                        ConferenceMedia = fullConference.ConferenceMedia?.Select(cfm => new DTOs.Conference.ConferenceMediaResponse
                        {
                            MediaId = cfm.ConferenceMediaId,
                            MediaUrl = cfm.ConferenceMediaUrl
                        }).ToList(),
                        ConferencePrices = fullConference.ConferencePrices?.Select(cp => new DTOs.Conference.ConferencePriceWithPhasesResponse
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
                                RefundPolicies = pp.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                                {
                                    RefundPolicyId = rp.RefundPolicyId,
                                    PercentRefund = rp.PercentRefund,
                                    RefundDeadline = rp.RefundDeadline,
                                    RefundOrder = rp.RefundOrder,
                                    PricePhaseID = pp.PricePhaseId
                                }).OrderBy(rp => rp.RefundOrder).ToList(),
                            }).ToList()
                        }).ToList()
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
                throw new BadRequestException($"Bạn chưa mua bất cứ vé nào nên không thể đánh giá");
            }
            var pendingCheckInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            if (pendingCheckInStatus == null)
            {
                throw new NotFoundException($"Không tìm thấy trạng thái check in trong hệ thống");
            }
            if (userCheckInFound.CheckinStatusId == pendingCheckInStatus.CheckinStatusId)
            {
                throw new BadRequestException($"Bạn phải check in rồi mới được đánh giá");
            }
            var conferenceFeedbackObj = new ConferenceFeedback()
            {
                ConferenceFeedbackId = Guid.NewGuid().ToString(),
                UserId = userId,
                ConferenceSessionId = request.ConferenceSessionId,
                Rating = request.Rating,
                Message = request.Message,
                CreatedAt = ExtensionHelper.GetVietnamTime(),
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
            return await _unitOfWork.ConferenceRepository.GetListConferencesForScheduleByUserId(userId, ExtensionHelper.GetVietnamDate(), readyStatusConference.ConferenceStatusId);
        }

        public async Task<List<ConferenceResponse>> GetConferenceByAssignedPapers(string? userId)
        {
            List<PaperReviewer> AssignPaper = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByUserIdAsync(userId);
            List<Conference> AssignedConference = AssignPaper.Select(ap => ap.Paper.Conference).OrderByDescending(c => c.CreatedAt).ToList();
            List<ConferenceResponse> responses = new();
            foreach (var conference in AssignedConference)
            {
                ConferenceResponse conferenceResponse = new ConferenceResponse
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
            if (conference.CreatedBy != userId) throw new BadRequestException("Bạn không có quyền gửi yêu cầu được approve cho hội nghị này");
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            if (conference.ConferenceStatusId != draftStatus.ConferenceStatusId) throw new BadRequestException($" conference với ID {confId} phải đang là draft status mới có thể yêu cầu duyệt được");
            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            if (conference.ConferenceStatusId == pendingStatus.ConferenceStatusId) throw new BadRequestException("Hội nghị đang chờ được duyệt!");
            return await UpdateConferenceStatusAsync(confId, pendingStatus.ConferenceStatusName, $"Collborator với ID: {userId} đang request conference với ID: {confId} để được duyệt");
        }

        // DÁN TOÀN BỘ PHIÊN BẢN NÀY ĐỂ THAY THẾ PHIÊN BẢN CŨ

        public async Task<bool> ActivateWaitlist(string confId, string userId)
        {
            #region === 1. LẤY DỮ LIỆU VÀ VALIDATION CƠ BẢN ===

            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conference == null)
                throw new NotFoundException($"Không tìm thấy hội nghị với ID {confId}");

            // 1.1. Phân quyền và loại hội nghị
            if (conference.CreatedBy != userId)
                throw new BadRequestException("Bạn không có quyền kích hoạt chế độ waitlist cho hội nghị này.");
            if (conference.IsResearchConference != true)
                throw new BadRequestException("Chức năng này chỉ dành cho hội nghị nghiên cứu.");

            // 1.2. Lấy các Phase và kiểm tra sự tồn tại
            var notWaitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseNotWaitListByConferenceIdAsync(confId);
            var waitlistPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseIsWaitListByConferenceIdAsync(confId);
            if (notWaitlistPhase == null || waitlistPhase == null)
                throw new InvalidOperationException("Hội nghị chưa được cấu hình đầy đủ phase chính và phase waitlist.");

            // 1.3. Kiểm tra xem waitlist đã được kích hoạt chưa
            if (waitlistPhase.IsActive == true) // Chỉ cần kiểm tra phase waitlist là đủ
                throw new BadRequestException($"Waitlist cho hội nghị này đã được kích hoạt trước đó.");

            // 1.4. Lấy Research Detail (cần cho các bước sau)
            var researchDetail = await _unitOfWork.ResearchConferenceDetailRepository.GetResearchConferenceDetailByConferenceIdAsync(confId);
            if (researchDetail == null)
                throw new InvalidOperationException($"Hội nghị chưa có chi tiết nghiên cứu (Research Detail).");

            #endregion

            #region === 2. VALIDATION LOGIC NGHIỆP VỤ ===

            // 2.1. Kiểm tra số lượng vé Author còn lại
            var authorConferencePrices = await _unitOfWork.ConferencePriceRepository.GetNumberOfIsAuthorByConferenceId(confId);
            var remainingAuthorSlots = authorConferencePrices.Sum(cp => cp.AvailableSlot ?? 0);
            if (remainingAuthorSlots <= 0)
                throw new BadRequestException("Không thể kích hoạt waitlist vì tất cả các suất dành cho tác giả (vé 'isAuthor') đã được bán hết.");

            // 2.2. Kiểm tra điều kiện thời gian
            var today = ExtensionHelper.GetVietnamDate();
            // 2.2a. Phải sau khi phase chính kết thúc hoàn toàn (kết thúc Camera Ready)
            if (today <= notWaitlistPhase.CameraReadyEndDate)
                throw new BadRequestException($"Không thể kích hoạt waitlist khi phase chính chưa kết thúc. Phase chính kết thúc vào ngày: {notWaitlistPhase.CameraReadyEndDate:dd/MM/yyyy}.");

            // 2.2b. Phải nằm trong khoảng thời gian đăng ký của phase waitlist
            if (today < waitlistPhase.RegistrationStartDate || today > waitlistPhase.RegistrationEndDate)
                throw new BadRequestException($"Chỉ có thể kích hoạt waitlist trong khoảng thời gian đăng ký của nó ({waitlistPhase.RegistrationStartDate:dd/MM/yyyy} - {waitlistPhase.RegistrationEndDate:dd/MM/yyyy}).");

            // 2.3. Kiểm tra xem người tổ chức đã tạo PricePhase cho vé Author trong giai đoạn Waitlist chưa
            var allAuthorPricePhases = await _unitOfWork.PricePhaseRepository.GetPricePhaseByconferenceIdThatIsAuthor(confId);
            bool hasPricePhaseForWaitlist = allAuthorPricePhases.Any(pp => pp.ResearchConferencePhaseId == waitlistPhase.ResearchConferencePhaseId);

            if (!hasPricePhaseForWaitlist)
                throw new BadRequestException("Không thể kích hoạt waitlist. Vui lòng tạo ít nhất một 'Giai đoạn bán vé' (Price Phase) cho loại vé 'isAuthor' có khoảng thời gian nằm trong giai đoạn đăng ký của waitlist.");

            #endregion

            #region === 3. THỰC THI THAY ĐỔI ===
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
    }
}