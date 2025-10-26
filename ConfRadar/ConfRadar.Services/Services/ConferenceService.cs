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
        Task<string> CreateConferenceAsync(CreateConferenceRequest request, string userId);
        Task<int> UpdateConferenceAsync(UpdateConferenceRequest request, string conferenceId);
        Task<int> DeleteConferenceAsync(string conferenceId);
        Task<ConferenceResponse> GetConferenceByIdAsync(string conferenceId);
        Task<List<ConferenceResponse>> GetAllConferencesAsync();
        Task<PagedResult<ConferenceResponse>> GetAllConferencesPaginatedAsync(int page, int pageSize);

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

        public async Task<string> CreateConferenceAsync(CreateConferenceRequest request, string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if user exists
                var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found");
                }

                // Check if user has organizer role
                //var userRoles = user.UserRoles;
                //var isOrganizer = userRoles.Any(ur => ur.Role.RoleName == "Conference Organizer");
                //if (!isOrganizer)
                //{
                //    throw new ConfRadarAuthenticationException("Only users with conference organizer role can create conferences");
                //}

                // Generate new conference ID
                var conferenceId = Guid.NewGuid().ToString();

                // Get or create category
                var category = await _unitOfWork.ConferenceCategoryRepository.GetCategoryByCategoryName(request.CategoryName);
                if (category == null)
                {
                    category = new ConferenceCategory
                    {
                        ConferenceCategoryId = Guid.NewGuid().ToString(),
                        ConferenceCategoryName = request.CategoryName
                    };
                    await _unitOfWork.ConferenceCategoryRepository.CreateConferenceCategoryAsync(category);
                }

                // Upload banner image if provided
                string? bannerImageUrl = null;
                if (request.BannerImageFile != null)
                {
                    // Validate file type
                    var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml" };
                    if (!allowedImageTypes.Contains(request.BannerImageFile.ContentType.ToLower()))
                    {
                        throw new BadRequestException("Only image files are allowed for banner");
                    }

                    using var stream = request.BannerImageFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                    bannerImageUrl = await _objectStorageFileService.UploadFileAsync(
                        ObjectStorageBucketEnum.conferencebanner.ToString(),
                        uniqueFileName,
                        stream,
                        request.BannerImageFile.ContentType);
                }

                // Create the conference
                var conference = new Conference
                {
                    ConferenceId = conferenceId,
                    ConferenceName = request.ConferenceName,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    //Capacity = request.Capacity,
                    Address = request.Address,
                    BannerImageUrl = bannerImageUrl,
                    //CreatedAt = DateTime.UtcNow,
                    IsInternalHosted = request.IsInternalHosted,
                    IsResearchConference = request.IsResearchConference,
                    //IsActive = true,
                    ConferenceCategoryId = category.ConferenceCategoryId,
                    //UserId = userId,
                    //LocationId = "", // Use existing location ID (fixed from hardcoded value)
                    //GlobalStatusId = request.GlobalStatusId
                };

                var result = await _unitOfWork.ConferenceRepository.CreateConferenceAsync(conference);

                if (result <= 0)
                {
                    throw new BadRequestException("Failed to create conference");
                }

                // Create price phase if provided
                string? pricePhaseId = null;
                if (request.PricePhase != null)
                {
                    var pricePhase = new PricePhase
                    {
                        PricePhaseId = Guid.NewGuid().ToString(),
                        //Name = request.PricePhase.Name,
                        //EarlierBirdEndInterval = request.PricePhase.EarlierBirdEndInterval,
                        //PercentForEarly = request.PricePhase.PercentForEarly,
                        //StandardEndInterval = request.PricePhase.StandardEndInterval,
                        //LateEndInterval = request.PricePhase.LateEndInterval,
                        //PercentForEnd = request.PricePhase.PercentForEnd
                    };
                    await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
                    pricePhaseId = pricePhase.PricePhaseId;
                }

                // Create prices if provided
                if (request.Prices != null && request.Prices.Any())
                {
                    foreach (var price in request.Prices)
                    {
                        string actualPricePhaseId = pricePhaseId; // Use the newly created price phase if available

                        if (string.IsNullOrEmpty(actualPricePhaseId))
                        {
                            // If no new price phase was created, validate existing one
                            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(price.PricePhaseId);
                            if (pricePhase == null)
                            {
                                throw new NotFoundException($"Price phase with ID {price.PricePhaseId} not found");
                            }
                            actualPricePhaseId = price.PricePhaseId;
                        }

                        var conferencePrice = new ConferencePrice
                        {
                            ConferencePriceId = Guid.NewGuid().ToString(),
                            TicketPrice = price.TicketPrice,
                            TicketName = price.TicketName,
                            TicketDescription = price.TicketDescription,
                            //ActualPrice = price.ActualPrice,
                            //PricePhaseId = actualPricePhaseId,
                            ConferenceId = conferenceId
                        };
                        await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(conferencePrice);
                    }
                }

                // Get configuration values for session validation
                var sessionConfig = await _systemConfigurationService.GetSessionConfigurationAsync();

                // Create sessions if provided - using existing rooms
                if (request.Sessions != null && request.Sessions.Any())
                {
                    foreach (var session in request.Sessions)
                    {
                        // Verify that the room exists before creating the session
                        if (session.RoomId != null)
                        {
                            var room = await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId);
                            if (room == null)
                            {
                                throw new NotFoundException($"Room with ID {session.RoomId} not found");
                            }

                            // Validate session time availability in the room
                            await ValidateSessionTimeAvailability(session, sessionConfig);
                        }

                        var conferenceSession = new ConferenceSession
                        {
                            ConferenceSessionId = Guid.NewGuid().ToString(),
                            Title = session.Title,
                            Description = session.Description,
                            StartTime = session.StartTime,
                            EndTime = session.EndTime,
                            // Date field has been removed, using StartTime and EndTime which contain date and time
                            ConferenceId = conferenceId,
                            RoomId = session.RoomId // Use existing room ID
                        };
                        await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                        // Create speaker if provided
                        if (session.Speaker != null)
                        {
                            var speaker = new Speaker
                            {
                                ConferenceSessionId = conferenceSession.ConferenceSessionId,
                                Name = session.Speaker.Name,
                                Description = session.Speaker.Description
                            };
                            await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
                        }
                    }
                }

                // Create conference policies if provided
                if (request.Policies != null && request.Policies.Any())
                {
                    foreach (var policy in request.Policies)
                    {
                        var conferencePolicy = new Policy
                        {
                            PolicyId = Guid.NewGuid().ToString(),
                            PolicyName = policy.PolicyName,
                            Description = policy.Description,
                            ConferenceId = conferenceId
                        };
                        await _unitOfWork.ConferencePolicyRepository.CreateConferencePolicyAsync(conferencePolicy);
                    }
                }

                // Create conference media if provided
                if (request.Media != null && request.Media.Any())
                {
                    foreach (var media in request.Media)
                    {
                        string? mediaUrl = media.MediaUrl; // Default to provided URL

                        if (media.MediaFile != null)
                        {
                            // Validate media type based on MediaType
                            //var mediaType = await _unitOfWork.MediaTypeRepository.GetMediaTypeByIdAsync(media.MediaTypeId);
                            //if (mediaType == null)
                            //{
                            //    throw new NotFoundException($"Media type with ID {media.MediaTypeId} not found");
                            //}

                            using var stream = media.MediaFile.OpenReadStream();
                            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(media.MediaFile.FileName);

                            // Determine bucket based on media type
                            var bucket = ObjectStorageBucketEnum.conferencemedia.ToString();

                            mediaUrl = await _objectStorageFileService.UploadFileAsync(
                                bucket,
                                uniqueFileName,
                                stream,
                                media.MediaFile.ContentType);
                        }

                        var conferenceMedia = new ConferenceMedium
                        {
                            ConferenceMediaId = Guid.NewGuid().ToString(),
                            ConferenceMediaUrl = mediaUrl,
                            ConferenceId = conferenceId,
                            //MediaTypeId = media.MediaTypeId
                        };
                        await _unitOfWork.ConferenceMediumRepository.CreateConferenceMediumAsync(conferenceMedia);
                    }
                }

                // Create sponsors if provided
                if (request.Sponsors != null && request.Sponsors.Any())
                {
                    foreach (var sponsor in request.Sponsors)
                    {
                        string? imageUrl = sponsor.ImageUrl; // Default to provided URL

                        // If an image file is provided, upload it to MinIO
                        if (sponsor.ImageFile != null)
                        {
                            using var stream = sponsor.ImageFile.OpenReadStream();
                            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(sponsor.ImageFile.FileName);
                            imageUrl = await _objectStorageFileService.UploadFileAsync(
                                ObjectStorageBucketEnum.conferencemedia.ToString(),
                                uniqueFileName,
                                stream,
                                sponsor.ImageFile.ContentType);
                        }

                        var conferenceSponsor = new Sponsor
                        {
                            SponsorId = Guid.NewGuid().ToString(),
                            Name = sponsor.Name,
                            ImageUrl = imageUrl,
                            ConferenceId = conferenceId
                        };
                        await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
                    }
                }

                await _unitOfWork.CommitAsync();
                return conferenceId;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw; // Re-throw the original exception
            }
        }

        public async Task<int> UpdateConferenceAsync(UpdateConferenceRequest request, string conferenceId)
        {
            var existingConference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (existingConference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // Update basic conference information
            existingConference.ConferenceName = request.ConferenceName ?? existingConference.ConferenceName;
            existingConference.Description = request.Description ?? existingConference.Description;
            existingConference.StartDate = request.StartDate ?? existingConference.StartDate;
            existingConference.EndDate = request.EndDate ?? existingConference.EndDate;
            //existingConference.Capacity = request.Capacity ?? existingConference.Capacity;
            existingConference.Address = request.Address ?? existingConference.Address;
            existingConference.BannerImageUrl = request.BannerImageUrl ?? existingConference.BannerImageUrl;
            existingConference.IsInternalHosted = request.IsInternalHosted ?? existingConference.IsInternalHosted;
            existingConference.IsResearchConference = request.IsResearchConference ?? existingConference.IsResearchConference;
            //existingConference.IsActive = request.IsActive ?? existingConference.IsActive;

            var result = await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(existingConference);
            return result;
        }

        public async Task<int> DeleteConferenceAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            // First delete related entities
            var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
            foreach (var policy in policies)
            {
                await _unitOfWork.ConferencePolicyRepository.DeleteConferencePolicyAsync(policy);
            }

            var media = await _unitOfWork.ConferenceMediumRepository.GetMediaByConferenceIdAsync(conferenceId);
            foreach (var m in media)
            {
                await _unitOfWork.ConferenceMediumRepository.DeleteConferenceMediumAsync(m);
            }

            var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
            foreach (var sponsor in sponsors)
            {
                await _unitOfWork.SponsorRepository.DeleteSponsorAsync(sponsor);
            }

            var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);

            foreach (var price in prices)
            {
                await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price);
                // Don't delete the PricePhase here - it might be used by other conferences
                // PricePhases should be managed separately or only deleted when no ConferencePrices reference them
            }



            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
            foreach (var session in sessions)
            {
                // Delete associated speaker
                var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(session.ConferenceSessionId);
                if (speaker != null)
                {
                    await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker);
                }

                await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session);
            }

            // Finally delete the conference itself
            var result = await _unitOfWork.ConferenceRepository.DeleteConferenceAsync(conference);
            return result;
        }

        public async Task<ConferenceResponse> GetConferenceByIdAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceWithDetailsAsync(conferenceId);
            if (conference == null)
            {
                throw new NotFoundException($"Conference with ID {conferenceId} not found");
            }

            var response = new ConferenceResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                //Capacity = conference.Capacity,
                Address = conference.Address,
                BannerImageUrl = AddBaseUrlToUrl(conference.BannerImageUrl),
                //CreatedAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                //IsActive = conference.IsActive,
                //UserId = conference.UserId,
                //LocationId = conference.LocationId,
                CategoryId = conference.ConferenceCategoryId,
                Policies = conference.Policies?.Select(p => new ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                //Media = conference.ConferenceMedia?.Select(m => new ConferenceMediaResponse
                //{
                //    MediaId = m.ConferenceMediaId,
                //    MediaUrl = AddBaseUrlToUrl(m.ConferenceMediaUrl),
                //    MediaTypeId = m.MediaTypeId
                //}).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = AddBaseUrlToUrl(s.ImageUrl)
                }).ToList(),
                Prices = conference.ConferencePrices?.Select(p =>
                {
                    // Calculate current phase and actual price based on current date and price phase
                    string currentPhase = "Unknown";
                    decimal? calculatedActualPrice = p.TicketPrice; // Default to the stored actual price

                    if (p.PricePhases != null) // If the price phase is loaded with the conference
                    {
                        var now = DateOnly.FromDateTime(DateTime.UtcNow);

                        //if (p.PricePhases.EarlierBirdEndInterval != null && now <= p.PricePhases.EarlierBirdEndInterval)
                        //{
                        //    currentPhase = "Early Bird";
                        //    if (p.PricePhases.PercentForEarly.HasValue && p.TicketPrice.HasValue)
                        //    {
                        //        calculatedActualPrice = p.TicketPrice * (p.PricePhases.PercentForEarly.Value / 100.0m);
                        //    }
                        //}
                        //else if (p.PricePhase.StandardEndInterval != null && now <= p.PricePhase.StandardEndInterval)
                        //{
                        //    currentPhase = "Standard";
                        //    calculatedActualPrice = p.TicketPrice; // Full price during standard phase
                        //}
                        //else if (p.PricePhase.LateEndInterval != null && now <= p.PricePhase.LateEndInterval)
                        //{
                        //    currentPhase = "Late";
                        //    if (p.PricePhase.PercentForEnd.HasValue && p.TicketPrice.HasValue)
                        //    {
                        //        calculatedActualPrice = p.TicketPrice * (p.PricePhase.PercentForEnd.Value / 100.0m);
                        //    }
                        //}
                        //else
                        //{
                        //    currentPhase = "Expired"; // After all phases ended
                        //}
                    }

                    return new ConferencePriceResponse
                    {
                        PriceId = p.ConferencePriceId,
                        TicketPrice = p.TicketPrice,
                        TicketName = p.TicketName,
                        TicketDescription = p.TicketDescription,
                        ActualPrice = calculatedActualPrice,
                        CurrentPhase = currentPhase,
                        //PricePhaseId = p.PricePhaseId
                    };
                }).ToList(),
                Sessions = conference.ConferenceSessions?.Select(s => new ConferenceSessionResponse
                {
                    SessionId = s.ConferenceSessionId,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    ConferenceId = s.ConferenceId,
                    RoomId = s.RoomId,
                    Room = s.Room != null ? new RoomInfoResponse
                    {
                        RoomId = s.Room.RoomId,
                        Number = s.Room.Number,
                        DisplayName = s.Room.DisplayName,
                        DestinationId = s.Room.DestinationId
                    } : null,
                    //Speaker = s.Speakers != null ? new SpeakerResponse
                    //{
                    //    Name = s.Speakers.Name,
                    //    Description = s.Speakers.Description
                    //} : null
                }).ToList()
            };

            return response;
        }

        /// <summary>
        /// Adds the base MinIO URL to a file URL if it's not already a full URL
        /// </summary>
        private string? AddBaseUrlToUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // If the URL already starts with http/https, return as is
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            // Prepend the base URL from configuration
            return _objectStorageSettings.EndPoint?.TrimEnd('/') + "/" + url.TrimStart('/');
        }

        public async Task<List<ConferenceResponse>> GetAllConferencesAsync()
        {
            var conferences = await _unitOfWork.ConferenceRepository.GetAllConferencesAsync();
            var responses = new List<ConferenceResponse>();

            foreach (var conference in conferences)
            {
                var response = new ConferenceResponse
                {
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    Description = conference.Description,
                    StartDate = conference.StartDate,
                    EndDate = conference.EndDate,
                    Capacity = conference.AvailableSlot,
                    Address = conference.Address,
                    BannerImageUrl = AddBaseUrlToUrl(conference.BannerImageUrl),
                    //CreatedAt = conference.CreatedAt,
                    IsInternalHosted = conference.IsInternalHosted,
                    IsResearchConference = conference.IsResearchConference,
                    //IsActive = conference.IsActive,
                    //UserId = conference.UserId,
                    //LocationId = conference.LocationId,
                    CategoryId = conference.ConferenceCategoryId
                };
                responses.Add(response);
            }

            return responses;
        }

        /// <summary>
        /// Validates that the session time is available in the room and meets duration/interval requirements
        /// </summary>
        private async Task ValidateSessionTimeAvailability(ConferenceSessionRequest session, SessionConfigurationResponse config)
        {
            if (session.StartTime == null || session.EndTime == null || session.RoomId == null)
            {
                return; // Skip validation if required fields are missing
            }

            // Convert to UTC for consistency
            var startTimeUtc = session.StartTime.Value.Kind == DateTimeKind.Utc ?
                session.StartTime.Value : DateTime.SpecifyKind(session.StartTime.Value, DateTimeKind.Utc);
            var endTimeUtc = session.EndTime.Value.Kind == DateTimeKind.Utc ?
                session.EndTime.Value : DateTime.SpecifyKind(session.EndTime.Value, DateTimeKind.Utc);

            // Check if session meets minimum duration requirement
            var sessionDuration = endTimeUtc - startTimeUtc;
            var minimumDuration = TimeSpan.FromHours(config.MinimumSessionDurationHours);

            if (sessionDuration < minimumDuration)
            {
                throw new BadRequestException($"Session duration must be at least {config.MinimumSessionDurationHours} hours");
            }

            // Get existing sessions in the same room on the same date for overlap checks
            var dateOnly = DateOnly.FromDateTime(startTimeUtc.Date);
            var existingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(session.RoomId, dateOnly);

            // Check for time overlaps with existing sessions
            foreach (var existingSession in existingSessions)
            {
                if (!existingSession.StartTime.HasValue || !existingSession.EndTime.HasValue) continue;

                // Ensure existing session times are also in UTC for comparison
                var existingStartUtc = existingSession.StartTime.Value.Kind == DateTimeKind.Utc ?
                    existingSession.StartTime.Value : DateTime.SpecifyKind(existingSession.StartTime.Value, DateTimeKind.Utc);
                var existingEndUtc = existingSession.EndTime.Value.Kind == DateTimeKind.Utc ?
                    existingSession.EndTime.Value : DateTime.SpecifyKind(existingSession.EndTime.Value, DateTimeKind.Utc);

                // Calculate session times with interval buffer
                var newSessionStartWithBuffer = startTimeUtc.AddHours(-config.SessionIntervalHours);
                var newSessionEndWithBuffer = endTimeUtc.AddHours(config.SessionIntervalHours);

                var existingSessionStartWithBuffer = existingStartUtc.AddHours(-config.SessionIntervalHours);
                var existingSessionEndWithBuffer = existingEndUtc.AddHours(config.SessionIntervalHours);

                // Check if there's an overlap (including interval buffer)
                if ((newSessionStartWithBuffer < existingSessionEndWithBuffer) &&
                    (newSessionEndWithBuffer > existingSessionStartWithBuffer))
                {
                    throw new BadRequestException($"Session conflicts with existing session in room {session.RoomId} at time {existingSession.StartTime?.ToString("HH:mm")} - {existingSession.EndTime?.ToString("HH:mm")}");
                }
            }
        }

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
                BannerImageUrl = AddBaseUrlToUrl(conference.BannerImageUrl),
                //CreatedAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                //IsActive = conference.IsActive,
                //UserId = conference.UserId,
                //LocationId = conference.LocationId,
                CategoryId = conference.ConferenceCategoryId,

            }).ToList();
            return new PagedResult<ConferenceResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}