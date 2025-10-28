using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Conference;
using ConfRadar.Services.DTOs.Configuration;
using ConfRadar.Services.DTOs.General;
using ConfRadar.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferenceService
    {
        //Task<string> CreateConferenceAsync(CreateConferenceRequest request);
        //Task<int> UpdateConferenceAsync(UpdateConferenceRequest request, string conferenceId);
        //Task<int> DeleteConferenceAsync(string conferenceId);
        //Task<ConferenceResponse> GetConferenceByIdAsync(string conferenceId);
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

        //public async Task<string> CreateConferenceAsync(CreateConferenceRequest request)
        //{
        //    await _unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        //// Check if user exists
        //        //var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
        //        //if (user == null)
        //        //{
        //        //    throw new NotFoundException($"User with ID {userId} not found");
        //        //}

        //        // Check if user has organizer role
        //        //var userRoles = user.UserRoles;
        //        //var isOrganizer = userRoles.Any(ur => ur.Role.RoleName == "Conference Organizer");
        //        //if (!isOrganizer)
        //        //{
        //        //    throw new ConfRadarAuthenticationException("Only users with conference organizer role can create conferences");
        //        //}

        //        // Generate new conference ID
        //        var conferenceId = Guid.NewGuid().ToString();

        //        // Get or create category
        //        var category = await _unitOfWork.ConferenceCategoryRepository.GetCategoryByCategoryName(request.Description);
        //        if (category == null)
        //        {
        //            category = new ConferenceCategory
        //            {
        //                ConferenceCategoryId = Guid.NewGuid().ToString(),
        //                //ConferenceCategoryName = request.CategoryName
        //            };
        //            await _unitOfWork.ConferenceCategoryRepository.CreateConferenceCategoryAsync(category);
        //        }

        //        // Upload banner image if provided
        //        string? bannerImageUrl = null;
        //        if (request.BannerImageFile != null)
        //        {
        //            // Validate file type
        //            var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml" };
        //            if (!allowedImageTypes.Contains(request.BannerImageFile.ContentType.ToLower()))
        //            {
        //                throw new BadRequestException("Only image files are allowed for banner");
        //            }

        //            using var stream = request.BannerImageFile.OpenReadStream();
        //            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
        //            bannerImageUrl = await _objectStorageFileService.UploadFileAsync(
        //                ObjectStorageBucketEnum.conferencebanner.ToString(),
        //                uniqueFileName,
        //                stream,
        //                request.BannerImageFile.ContentType);
        //        }

        //        // Create the conference
        //        var conference = new Conference
        //        {
        //            ConferenceId = conferenceId,
        //            ConferenceName = request.ConferenceName,
        //            Description = request.Description,
        //            StartDate = request.StartDate,
        //            EndDate = request.EndDate,
        //            //Capacity = request.Capacity,
        //            Address = request.Address,
        //            BannerImageUrl = bannerImageUrl,
        //            //CreatedAt = DateTime.UtcNow,
        //            IsInternalHosted = request.IsInternalHosted,
        //            IsResearchConference = request.IsResearchConference,
        //            //IsActive = true,
        //            ConferenceCategoryId = category.ConferenceCategoryId,
        //            //UserId = userId,
        //            //LocationId = "", // Use existing location ID (fixed from hardcoded value)
        //            //GlobalStatusId = request.GlobalStatusId
        //        };

        //        var result = await _unitOfWork.ConferenceRepository.CreateConferenceAsync(conference);

        //        if (result <= 0)
        //        {
        //            throw new BadRequestException("Failed to create conference");
        //        }

        //        // Create price phase if provided
        //        string? pricePhaseId = null;
        //        if (request.PricePhase != null)
        //        {
        //            var pricePhase = new PricePhase
        //            {
        //                PricePhaseId = Guid.NewGuid().ToString(),
        //                //Name = request.PricePhase.Name,
        //                //EarlierBirdEndInterval = request.PricePhase.EarlierBirdEndInterval,
        //                //PercentForEarly = request.PricePhase.PercentForEarly,
        //                //StandardEndInterval = request.PricePhase.StandardEndInterval,
        //                //LateEndInterval = request.PricePhase.LateEndInterval,
        //                //PercentForEnd = request.PricePhase.PercentForEnd
        //            };
        //            await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
        //            pricePhaseId = pricePhase.PricePhaseId;
        //        }

        //        // Create prices if provided
        //        if (request.Prices != null && request.Prices.Any())
        //        {
        //            foreach (var price in request.Prices)
        //            {
        //                string actualPricePhaseId = pricePhaseId; // Use the newly created price phase if available

        //                if (string.IsNullOrEmpty(actualPricePhaseId))
        //                {
        //                    // If no new price phase was created, validate existing one
        //                    var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(price.PricePhaseId);
        //                    if (pricePhase == null)
        //                    {
        //                        throw new NotFoundException($"Price phase with ID {price.PricePhaseId} not found");
        //                    }
        //                    actualPricePhaseId = price.PricePhaseId;
        //                }

        //                var conferencePrice = new ConferencePrice
        //                {
        //                    ConferencePriceId = Guid.NewGuid().ToString(),
        //                    TicketPrice = price.TicketPrice,
        //                    TicketName = price.TicketName,
        //                    TicketDescription = price.TicketDescription,
        //                    //ActualPrice = price.ActualPrice,
        //                    //PricePhaseId = actualPricePhaseId,
        //                    ConferenceId = conferenceId
        //                };
        //                await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(conferencePrice);
        //            }
        //        }

        //        // Get configuration values for session validation
        //        var sessionConfig = await _systemConfigurationService.GetSessionConfigurationAsync();

        //        // Create sessions if provided - using existing rooms
        //        if (request.Sessions != null && request.Sessions.Any())
        //        {
        //            foreach (var session in request.Sessions)
        //            {
        //                // Verify that the room exists before creating the session
        //                if (session.RoomId != null)
        //                {
        //                    var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId);
        //                    if (room == null)
        //                    {
        //                        throw new NotFoundException($"Room with ID {session.RoomId} not found");
        //                    }

        //                    // Validate session time availability in the room
        //                    await ValidateSessionTimeAvailability(session, sessionConfig);
        //                }

        //                var conferenceSession = new ConferenceSession
        //                {
        //                    ConferenceSessionId = Guid.NewGuid().ToString(),
        //                    Title = session.Title,
        //                    Description = session.Description,
        //                    StartTime = session.StartTime.HasValue ? 
        //                        (session.StartTime.Value.Kind == DateTimeKind.Unspecified ?
        //                         session.StartTime.Value : DateTime.SpecifyKind(session.StartTime.Value, DateTimeKind.Unspecified)) : null,
        //                    EndTime = session.EndTime.HasValue ?
        //                        (session.EndTime.Value.Kind == DateTimeKind.Unspecified ?
        //                         session.EndTime.Value : DateTime.SpecifyKind(session.EndTime.Value, DateTimeKind.Unspecified)) : null,
        //                    // Date field has been removed, using StartTime and EndTime which contain date and time
        //                    ConferenceId = conferenceId,
        //                    RoomId = session.RoomId // Use existing room ID
        //                };
        //                await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

        //                // Create speaker if provided
        //                if (session.Speaker != null)
        //                {
        //                    var speaker = new Speaker
        //                    {
        //                        ConferenceSessionId = conferenceSession.ConferenceSessionId,
        //                        Name = session.Speaker.Name,
        //                        Description = session.Speaker.Description
        //                    };
        //                    await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
        //                }
        //            }
        //        }

        //        // Create conference policies if provided
        //        if (request.Policies != null && request.Policies.Any())
        //        {
        //            foreach (var policy in request.Policies)
        //            {
        //                var conferencePolicy = new Policy
        //                {
        //                    PolicyId = Guid.NewGuid().ToString(),
        //                    PolicyName = policy.PolicyName,
        //                    Description = policy.Description,
        //                    ConferenceId = conferenceId
        //                };
        //                await _unitOfWork.ConferencePolicyRepository.CreateConferencePolicyAsync(conferencePolicy);
        //            }
        //        }

        //        // Create conference media if provided
        //        if (request.Media != null && request.Media.Any())
        //        {
        //            foreach (var media in request.Media)
        //            {
        //                string? mediaUrl = media.MediaUrl; // Default to provided URL

        //                if (media.MediaFile != null)
        //                {
        //                    // Validate media type based on MediaType
        //                    //var mediaType = await _unitOfWork.MediaTypeRepository.GetMediaTypeByIdAsync(media.MediaTypeId);
        //                    //if (mediaType == null)
        //                    //{
        //                    //    throw new NotFoundException($"Media type with ID {media.MediaTypeId} not found");
        //                    //}

        //                    using var stream = media.MediaFile.OpenReadStream();
        //                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(media.MediaFile.FileName);

        //                    // Determine bucket based on media type
        //                    var bucket = ObjectStorageBucketEnum.conferencemedia.ToString();

        //                    mediaUrl = await _objectStorageFileService.UploadFileAsync(
        //                        bucket,
        //                        uniqueFileName,
        //                        stream,
        //                        media.MediaFile.ContentType);
        //                }

        //                var conferenceMedia = new ConferenceMedium
        //                {
        //                    ConferenceMediaId = Guid.NewGuid().ToString(),
        //                    ConferenceMediaUrl = mediaUrl,
        //                    ConferenceId = conferenceId,
        //                    //MediaTypeId = media.MediaTypeId
        //                };
        //                await _unitOfWork.ConferenceMediumRepository.CreateConferenceMediumAsync(conferenceMedia);
        //            }
        //        }

        //        // Create sponsors if provided
        //        if (request.Sponsors != null && request.Sponsors.Any())
        //        {
        //            foreach (var sponsor in request.Sponsors)
        //            {
        //                string? imageUrl = sponsor.ImageUrl; // Default to provided URL

        //                // If an image file is provided, upload it to MinIO
        //                if (sponsor.ImageFile != null)
        //                {
        //                    using var stream = sponsor.ImageFile.OpenReadStream();
        //                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(sponsor.ImageFile.FileName);
        //                    imageUrl = await _objectStorageFileService.UploadFileAsync(
        //                        ObjectStorageBucketEnum.conferencemedia.ToString(),
        //                        uniqueFileName,
        //                        stream,
        //                        sponsor.ImageFile.ContentType);
        //                }

        //                var conferenceSponsor = new Sponsor
        //                {
        //                    SponsorId = Guid.NewGuid().ToString(),
        //                    Name = sponsor.Name,
        //                    ImageUrl = imageUrl,
        //                    ConferenceId = conferenceId
        //                };
        //                await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
        //            }
        //        }

        //        await _unitOfWork.CommitAsync();
        //        return conferenceId;
        //    }
        //    catch (Exception)
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw; // Re-throw the original exception
        //    }
        //}

        //public async Task<int> UpdateConferenceAsync(UpdateConferenceRequest request, string conferenceId)
        //{
        //    var existingConference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
        //    if (existingConference == null)
        //    {
        //        throw new NotFoundException($"Conference with ID {conferenceId} not found");
        //    }

        //    // Update basic conference information
        //    existingConference.ConferenceName = request.ConferenceName ?? existingConference.ConferenceName;
        //    existingConference.Description = request.Description ?? existingConference.Description;
        //    existingConference.StartDate = request.StartDate ?? existingConference.StartDate;
        //    existingConference.EndDate = request.EndDate ?? existingConference.EndDate;
        //    //existingConference.Capacity = request.Capacity ?? existingConference.Capacity;
        //    existingConference.Address = request.Address ?? existingConference.Address;
        //    existingConference.BannerImageUrl = request.BannerImageUrl ?? existingConference.BannerImageUrl;
        //    existingConference.IsInternalHosted = request.IsInternalHosted ?? existingConference.IsInternalHosted;
        //    existingConference.IsResearchConference = request.IsResearchConference ?? existingConference.IsResearchConference;
        //    //existingConference.IsActive = request.IsActive ?? existingConference.IsActive;

        //    var result = await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(existingConference);
        //    return result;
        //}

        //public async Task<int> DeleteConferenceAsync(string conferenceId)
        //{
        //    var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
        //    if (conference == null)
        //    {
        //        throw new NotFoundException($"Conference with ID {conferenceId} not found");
        //    }

        //    // First delete related entities
        //    var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
        //    foreach (var policy in policies)
        //    {
        //        await _unitOfWork.ConferencePolicyRepository.DeleteConferencePolicyAsync(policy);
        //    }

        //    var media = await _unitOfWork.ConferenceMediumRepository.GetMediaByConferenceIdAsync(conferenceId);
        //    foreach (var m in media)
        //    {
        //        await _unitOfWork.ConferenceMediumRepository.DeleteConferenceMediumAsync(m);
        //    }

        //    var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
        //    foreach (var sponsor in sponsors)
        //    {
        //        await _unitOfWork.SponsorRepository.DeleteSponsorAsync(sponsor);
        //    }

        //    var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);

        //    foreach (var price in prices)
        //    {
        //        await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price);
        //        // Don't delete the PricePhase here - it might be used by other conferences
        //        // PricePhases should be managed separately or only deleted when no ConferencePrices reference them
        //    }



        //    var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
        //    foreach (var session in sessions)
        //    {
        //        // Delete associated speaker
        //        var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(session.ConferenceSessionId);
        //        if (speaker != null)
        //        {
        //            await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker);
        //        }

        //        await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session);
        //    }

        //    // Finally delete the conference itself
        //    var result = await _unitOfWork.ConferenceRepository.DeleteConferenceAsync(conference);
        //    return result;
        //}

        //public async Task<ConferenceResponse> GetConferenceByIdAsync(string conferenceId)
        //{
        //    var conference = await _unitOfWork.ConferenceRepository.GetConferenceWithDetailsAsync(conferenceId);
        //    if (conference == null)
        //    {
        //        throw new NotFoundException($"Conference with ID {conferenceId} not found");
        //    }

        //    var response = new ConferenceResponse
        //    {
        //        ConferenceId = conference.ConferenceId,
        //        ConferenceName = conference.ConferenceName,
        //        Description = conference.Description,
        //        StartDate = conference.StartDate,
        //        EndDate = conference.EndDate,
        //        //Capacity = conference.Capacity,
        //        Address = conference.Address,
        //        BannerImageUrl = AddBaseUrlToUrl(conference.BannerImageUrl),
        //        //CreatedAt = conference.CreatedAt,
        //        IsInternalHosted = conference.IsInternalHosted,
        //        IsResearchConference = conference.IsResearchConference,
        //        //IsActive = conference.IsActive,
        //        //UserId = conference.UserId,
        //        //LocationId = conference.LocationId,
        //        CategoryId = conference.ConferenceCategoryId,
        //        Policies = conference.Policies?.Select(p => new ConferencePolicyResponse
        //        {
        //            PolicyId = p.PolicyId,
        //            PolicyName = p.PolicyName,
        //            Description = p.Description
        //        }).ToList(),
        //        //Media = conference.ConferenceMedia?.Select(m => new ConferenceMediaResponse
        //        //{
        //        //    MediaId = m.ConferenceMediaId,
        //        //    MediaUrl = AddBaseUrlToUrl(m.ConferenceMediaUrl),
        //        //    MediaTypeId = m.MediaTypeId
        //        //}).ToList(),
        //        Sponsors = conference.Sponsors?.Select(s => new SponsorResponse
        //        {
        //            SponsorId = s.SponsorId,
        //            Name = s.Name,
        //            ImageUrl = AddBaseUrlToUrl(s.ImageUrl)
        //        }).ToList(),
        //        Prices = conference.ConferencePrices?.Select(p =>
        //        {
        //            // Calculate current phase and actual price based on current date and price phase
        //            string currentPhase = "Unknown";
        //            decimal? calculatedActualPrice = p.TicketPrice; // Default to the stored actual price

        //            if (p.PricePhases != null) // If the price phase is loaded with the conference
        //            {
        //                var now = DateOnly.FromDateTime(DateTime.UtcNow);

        //                //if (p.PricePhases.EarlierBirdEndInterval != null && now <= p.PricePhases.EarlierBirdEndInterval)
        //                //{
        //                //    currentPhase = "Early Bird";
        //                //    if (p.PricePhases.PercentForEarly.HasValue && p.TicketPrice.HasValue)
        //                //    {
        //                //        calculatedActualPrice = p.TicketPrice * (p.PricePhases.PercentForEarly.Value / 100.0m);
        //                //    }
        //                //}
        //                //else if (p.PricePhase.StandardEndInterval != null && now <= p.PricePhase.StandardEndInterval)
        //                //{
        //                //    currentPhase = "Standard";
        //                //    calculatedActualPrice = p.TicketPrice; // Full price during standard phase
        //                //}
        //                //else if (p.PricePhase.LateEndInterval != null && now <= p.PricePhase.LateEndInterval)
        //                //{
        //                //    currentPhase = "Late";
        //                //    if (p.PricePhase.PercentForEnd.HasValue && p.TicketPrice.HasValue)
        //                //    {
        //                //        calculatedActualPrice = p.TicketPrice * (p.PricePhase.PercentForEnd.Value / 100.0m);
        //                //    }
        //                //}
        //                //else
        //                //{
        //                //    currentPhase = "Expired"; // After all phases ended
        //                //}
        //            }

        //            return new ConferencePriceResponse
        //            {
        //                PriceId = p.ConferencePriceId,
        //                TicketPrice = p.TicketPrice,
        //                TicketName = p.TicketName,
        //                TicketDescription = p.TicketDescription,
        //                ActualPrice = calculatedActualPrice,
        //                CurrentPhase = currentPhase,
        //                //PricePhaseId = p.PricePhaseId
        //            };
        //        }).ToList(),
        //        Sessions = conference.ConferenceSessions?.Select(s => new ConferenceSessionResponse
        //        {
        //            SessionId = s.ConferenceSessionId,
        //            Title = s.Title,
        //            Description = s.Description,
        //            StartTime = s.StartTime,
        //            EndTime = s.EndTime,
        //            ConferenceId = s.ConferenceId,
        //            RoomId = s.RoomId,
        //            Room = s.Room != null ? new RoomInfoResponse
        //            {
        //                RoomId = s.Room.RoomId,
        //                Number = s.Room.Number,
        //                DisplayName = s.Room.DisplayName,
        //                DestinationId = s.Room.DestinationId
        //            } : null,
        //            //Speaker = s.Speakers != null ? new SpeakerResponse
        //            //{
        //            //    Name = s.Speakers.Name,
        //            //    Description = s.Speakers.Description
        //            //} : null
        //        }).ToList()
        //    };

        //    return response;
        //}

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

        //public async Task<List<ConferenceResponse>> GetAllConferencesAsync()
        //{
        //    var conferences = await _unitOfWork.ConferenceRepository.GetAllConferencesAsync();
        //    var responses = new List<ConferenceResponse>();

        //    foreach (var conference in conferences)
        //    {
        //        var response = new ConferenceResponse
        //        {
        //            ConferenceId = conference.ConferenceId,
        //            ConferenceName = conference.ConferenceName,
        //            Description = conference.Description,
        //            StartDate = conference.StartDate,
        //            EndDate = conference.EndDate,
        //            Capacity = conference.AvailableSlot,
        //            Address = conference.Address,
        //            BannerImageUrl = AddBaseUrlToUrl(conference.BannerImageUrl),
        //            //CreatedAt = conference.CreatedAt,
        //            IsInternalHosted = conference.IsInternalHosted,
        //            IsResearchConference = conference.IsResearchConference,
        //            //IsActive = conference.IsActive,
        //            //UserId = conference.UserId,
        //            //LocationId = conference.LocationId,
        //            CategoryId = conference.ConferenceCategoryId
        //        };
        //        responses.Add(response);
        //    }

        //    return responses;
        //}

        ///// <summary>
        ///// Validates that the session time is available in the room and meets duration/interval requirements
        ///// </summary>
        //private async Task ValidateSessionTimeAvailability(ConferenceSessionRequest session, SessionConfigurationResponse config)
        //{
        //    if (session.StartTime == null || session.EndTime == null || session.RoomId == null)
        //    {
        //        return; // Skip validation if required fields are missing
        //    }

        //    // Convert to Unspecified for consistency with PostgreSQL timestamp without timezone
        //    var startTimeUnspecified = session.StartTime.Value.Kind == DateTimeKind.Unspecified ?
        //        session.StartTime.Value : DateTime.SpecifyKind(session.StartTime.Value, DateTimeKind.Unspecified);
        //    var endTimeUnspecified = session.EndTime.Value.Kind == DateTimeKind.Unspecified ?
        //        session.EndTime.Value : DateTime.SpecifyKind(session.EndTime.Value, DateTimeKind.Unspecified);

        //    // Check if session meets minimum duration requirement
        //    var sessionDuration = endTimeUtc - startTimeUtc;
        //    var minimumDuration = TimeSpan.FromHours(config.MinimumSessionDurationHours);

        //    if (sessionDuration < minimumDuration)
        //    {
        //        throw new BadRequestException($"Session duration must be at least {config.MinimumSessionDurationHours} hours");
        //    }

        //    // Get existing sessions in the same room on the same date for overlap checks
        //    var dateOnly = DateOnly.FromDateTime(startTimeUtc.Date);
        //    var existingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(session.RoomId, dateOnly);

        //    // Check for time overlaps with existing sessions
        //    foreach (var existingSession in existingSessions)
        //    {
        //        if (!existingSession.StartTime.HasValue || !existingSession.EndTime.HasValue) continue;

        //        // Ensure existing session times are in Unspecified for comparison with PostgreSQL timestamp without timezone
        //        var existingStartUnspecified = existingSession.StartTime.Value.Kind == DateTimeKind.Unspecified ?
        //            existingSession.StartTime.Value : DateTime.SpecifyKind(existingSession.StartTime.Value, DateTimeKind.Unspecified);
        //        var existingEndUnspecified = existingSession.EndTime.Value.Kind == DateTimeKind.Unspecified ?
        //            existingSession.EndTime.Value : DateTime.SpecifyKind(existingSession.EndTime.Value, DateTimeKind.Unspecified);

        //        // Calculate session times with interval buffer
        //        var newSessionStartWithBuffer = startTimeUtc.AddHours(-config.SessionIntervalHours);
        //        var newSessionEndWithBuffer = endTimeUtc.AddHours(config.SessionIntervalHours);

        //        var existingSessionStartWithBuffer = existingStartUtc.AddHours(-config.SessionIntervalHours);
        //        var existingSessionEndWithBuffer = existingEndUtc.AddHours(config.SessionIntervalHours);

        //        // Check if there's an overlap (including interval buffer)
        //        if ((newSessionStartWithBuffer < existingSessionEndWithBuffer) &&
        //            (newSessionEndWithBuffer > existingSessionStartWithBuffer))
        //        {
        //            throw new BadRequestException($"Session conflicts with existing session in room {session.RoomId} at time {existingSession.StartTime?.ToString("HH:mm")} - {existingSession.EndTime?.ToString("HH:mm")}");
        //        }
        //    }
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
                query = query.Where(c => c.ConferenceName.Contains(searchKeyword) || c.Description.Contains(searchKeyword));
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
                ConferencePrices = conference.ConferencePrices?.Select(cp => new ConferencePriceWithPhasesResponse
                {
                    ConferencePriceId = cp.ConferencePriceId,
                    TicketPrice = cp.TicketPrice,
                    TicketName = cp.TicketName,
                    TicketDescription = cp.TicketDescription,
                    IsAuthor = cp.IsAuthor,
                    TotalSlot = cp.TotalSlot,
                    AvailableSlot = cp.AvailableSlot,
                    PricePhases = cp.PricePhases?.Select(pp => new PricePhaseResponse
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
                Policies = conference.Policies?.Select(p => new ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new SponsorResponse
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
                    Speakers = cs.Speakers?.Select(s => new SpeakerResponse
                    {
                        SpeakerId = s.SpeakerId,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image
                    }).ToList(),
                    SessionMedia = cs.ConferenceSessionMedia?.Select(csm => new ConferenceSessionMediaResponse
                    {
                        ConferenceSessionMediaId = csm.ConferenceSessionMediaId,
                        ConferenceSessionMediaUrl = csm.MediaUrl
                    }).ToList()
                }).ToList(),
                ConferencePrices = conference.ConferencePrices?.Select(cp => new ConferencePriceWithPhasesResponse
                {
                    ConferencePriceId = cp.ConferencePriceId,
                    TicketPrice = cp.TicketPrice,
                    TicketName = cp.TicketName,
                    TicketDescription = cp.TicketDescription,
                    IsAuthor = cp.IsAuthor,
                    TotalSlot = cp.TotalSlot,
                    AvailableSlot = cp.AvailableSlot,
                    PricePhases = cp.PricePhases?.Select(pp => new PricePhaseResponse
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
                query = query.Where(c => c.ConferenceName.Contains(searchKeyword) || c.Description.Contains(searchKeyword));
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
                query = query.Where(c => c.ConferenceName.Contains(searchKeyword) || c.Description.Contains(searchKeyword));
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
    }
}