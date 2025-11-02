using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.DTOs.General;
using ConfRadar.Services.Exceptions;
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
        Task<TechnicalConferenceDetailResponse> GetTechnicalConferenceDetailAsync(string conferenceId);

        // Endpoint 3: Get conferences by status ID with filtering
        Task<PagedResult<ConferenceResponse>> GetConferencesByStatusAsync(string conferenceStatusId, int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // Endpoint 4: Get conferences with step completion status
        Task<PagedResult<ConferenceStepCompletionStatusResponse>> GetConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // NEW ENDPOINT 5: Get all pending conferences
        Task<PagedResult<ConferenceResponse>> GetPendingConferencesAsync(int page, int pageSize, string? searchKeyword = null);

        // NEW ENDPOINT 6: Approve conference (change status from pending to preparing)
        Task<bool> ApproveConferenceAsync(string conferenceId, ApproveConferenceRequest request);

        // Helper method: Update conference status
        Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null);

        // NEW ENDPOINT 7: Get detailed research conference data
        Task<DTOs.Conference.ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId);

        // NEW ENDPOINT 8: Get research conference step completion status
        Task<PagedResult<DTOs.Conference.ResearchConferenceStepCompletionStatusResponse>> GetResearchConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null);

        // NEW ENDPOINT 9: Check if technical conference has completed a specific step
        Task<bool> CheckTechnicalConferenceStepCompletionAsync(string conferenceId, string step);

        // NEW ENDPOINT 10: Check if research conference has completed a specific step
        Task<bool> CheckResearchConferenceStepCompletionAsync(string conferenceId, string step);
    }

    public class ConferenceService : IConferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly ISystemConfigurationService _systemConfigurationService;

        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceService(IUnitOfWork unitOfWork, IObjectStorageFileService objectStorageFileService, ITokenService tokenService, ISystemConfigurationService systemConfigurationService, IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
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



        public async Task<PagedResult<ConferenceResponse>> GetAllConferencesPaginatedAsync(int page, int pageSize)
        {
            var query = _unitOfWork.ConferenceRepository.GetAllConferences();

            var totalCount = await query.CountAsync();

            var pagedConferences = await query
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
            IQueryable<Conference> query = _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases);

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
                        AvailableSlot = pp.AvailableSlot
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

        public async Task<TechnicalConferenceDetailResponse> GetTechnicalConferenceDetailAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
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
                RefundPolicies = conference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                {
                    RefundPolicyId = rp.RefundPolicyId,
                    PercentRefund = rp.PercentRefund,
                    RefundDeadline = rp.RefundDeadline,
                    RefundOrder = rp.RefundOrder
                }).ToList(),
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
                        AvailableSlot = pp.AvailableSlot
                    }).ToList()
                }).ToList()
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

        public async Task<PagedResult<ConferenceStepCompletionStatusResponse>> GetConferencesStepCompletionStatusAsync(int page, int pageSize, string? searchKeyword = null, string? cityId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _unitOfWork.ConferenceRepository.GetAllConferences();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(c => c.ConferenceName.Contains(searchKeyword) || c.Description.ToLower().Contains(searchKeyword.ToLower()));
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

        public async Task<bool> UpdateConferenceStatusAsync(string conferenceId, string newStatusName, string? reason = null)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                return false;
            }

            // Get the new status by name
            var allStatuses = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
            var newStatus = allStatuses.FirstOrDefault(s => s.ConferenceStatusName == newStatusName);

            if (newStatus == null)
            {
                return false;
            }

            // Update the conference status
            conference.ConferenceStatusId = newStatus.ConferenceStatusId;

            // Here we could use the reason parameter in the future to store in a history/timeline table
            // For now, we're just keeping the field for future use as requested

            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            return true;
        }

        public async Task<DTOs.Conference.ResearchConferenceDetailResponse> GetResearchConferenceDetailAsync(string conferenceId)
        {
            // Get the main conference with related data
            var conference = await _unitOfWork.ConferenceRepository.GetAllConferences()
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
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
            var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(conferenceId);

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
                RefundPolicies = conference.RefundPolicies?.Select(rp => new DTOs.Conference.RefundPolicyResponse
                {
                    RefundPolicyId = rp.RefundPolicyId,
                    PercentRefund = rp.PercentRefund,
                    RefundDeadline = rp.RefundDeadline,
                    RefundOrder = rp.RefundOrder
                }).ToList(),
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
                        AvailableSlot = pp.AvailableSlot
                    }).ToList()
                }).ToList()
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

                var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(conference.ConferenceId);
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
                    var researchPhase = await _unitOfWork.ResearchConferencePhaseRepository.GetResearchConferencePhaseByConferenceIdAsync(conferenceId);
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
    }
}