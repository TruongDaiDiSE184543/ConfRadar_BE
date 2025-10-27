using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStepService
    {
        Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid);
        // Step 1: Basic Conference Creation
        Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId);
        Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request);

        // Step 2: Add Conference Prices
        Task<List<ConferencePriceWithPhasesResponse>> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request);
        Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId);
        Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request);
        Task<bool> DeleteConferencePriceAsync(string priceId);

        // Step 3: Add Conference Sessions
        Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request);
        Task<List<ConferenceSessionWithMediaResponse>> GetConferenceSessionsAsync(string conferenceId);
        Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request);
        Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request);
        Task<bool> DeleteConferenceSessionAsync(string sessionId);

        // Step 4: Add Conference Policies
        Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request);
        Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId);
        Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request);
        Task<bool> DeleteConferencePolicyAsync(string policyId);

        // Step 5: Add Conference Media
        Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request);
        Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId);
        Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request);
        Task<bool> DeleteConferenceMediaAsync(string mediaId);

        // Step 6: Add Conference Sponsors
        Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request);
        Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId);
        Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request);
        Task<bool> DeleteSponsorAsync(string sponsorId);
    }

    public class ConferenceStepService : IConferenceStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageFileService _objectStorageFileService;
        private readonly ITokenService _tokenService;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferenceStepService(
            IUnitOfWork unitOfWork,
            IObjectStorageFileService objectStorageFileService,
            ITokenService tokenService,
            IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _objectStorageFileService = objectStorageFileService;
            _tokenService = tokenService;
            _objectStorageSettings = objectStorageSettings.Value;
        }

        #region Helper Methods

        private string? AddBaseUrlToUrl(string? url)
        {
            if (string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            return _objectStorageSettings.EndPoint?.TrimEnd('/') + "/" + url.TrimStart('/');
        }

        private async Task ValidateSessionTimeAvailability(DateTime startTime, DateTime endTime, string roomId, string? sessionIdToExclude = null)
        {
            var startTimeUtc = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            var endTimeUtc = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);

            if ((endTimeUtc - startTimeUtc).TotalMinutes < 30)
            {
                throw new BadRequestException("Session duration must be at least 30 minutes.");
            }

            var sessionDate = DateOnly.FromDateTime(startTimeUtc);
            var existingSessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByRoomIdOnDateAsync(roomId, sessionDate);

            foreach (var existingSession in existingSessions)
            {
                if (existingSession.ConferenceSessionId == sessionIdToExclude) continue;
                if (!existingSession.StartTime.HasValue || !existingSession.EndTime.HasValue) continue;

                var existingStartUtc = DateTime.SpecifyKind(existingSession.StartTime.Value, DateTimeKind.Unspecified);
                var existingEndUtc = DateTime.SpecifyKind(existingSession.EndTime.Value, DateTimeKind.Unspecified);

                if (startTimeUtc < existingEndUtc && endTimeUtc > existingStartUtc)
                {
                    throw new BadRequestException($"Session conflicts with an existing session in room {roomId} from {existingStartUtc:HH:mm} to {existingEndUtc:HH:mm}.");
                }
            }
        }

        #endregion

        #region Step 1: Basic Conference

        public async Task<TechnicalConferenceBasicStepResponse> CreateTechnicalConferenceBasicAsync(CreateTechnicalConferenceBasicRequest request, string userid)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = await _unitOfWork.ConferenceCategoryRepository.GetConferenceCategoryByIdAsync(request.ConferenceCategoryId);
                if (category == null)
                {
                    throw new Exception($"Category {request.ConferenceCategoryId} does not exist");
                }
                var bannerExtension = request.BannerImageFile?.ContentType switch
                {
                    "image/jpeg" => "jpeg",
                    "image/gif" => "gif",
                    "image/png" => "png",
                    _ => null
                };
                request.createdby = userid;
                if (bannerExtension == null && request.BannerImageFile != null) throw new Exception("BannerImageFile extension is not supported");
                if (request.BannerImageFile != null)
                {
                    using var stream = request.BannerImageFile.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                    request.bannerImageFileUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
                    request.bannerImageFileUrl = _objectStorageSettings.EndPoint + request.bannerImageFileUrl;
                }
                if (request.StartDate < DateOnly.FromDateTime(DateTime.Today) &&
                    request.EndDate < DateOnly.FromDateTime(DateTime.Today) &&
                    request.TicketSaleEnd < DateOnly.FromDateTime(DateTime.Today) &&
                    request.TicketSaleStart < DateOnly.FromDateTime(DateTime.Today)
                    ) throw new Exception("Date must be after today");
                if (request.StartDate > request.EndDate || request.TicketSaleStart > request.TicketSaleEnd ||
                    request.TicketSaleEnd > request.StartDate) throw new Exception("date start must be after dateend the same with ticketsale and ticketsale end must be before date start ");
                if (request.TotalSlot < 0) throw new Exception("Total slot must be positive");
                var vietNamTimeZoneNow = DateOnly.FromDateTime(DateTime.Now);
                var userRole = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userid);
                var OrganizerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Conference Organizer");
                var roleOfUser = userRole.Select(S => S.RoleId);
                Conference toBeCreatedConference;
                var confStatus = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
                if (roleOfUser.Contains(OrganizerRole.RoleId)) toBeCreatedConference = ConferenceStepBasicCreateToModel.creatBasicConference(request, confStatus.Where(s => s.ConferenceStatusName == "Preparing").FirstOrDefault(), vietNamTimeZoneNow);
                else toBeCreatedConference = ConferenceStepBasicCreateToModel.creatBasicConference(request, confStatus.Where(s => s.ConferenceStatusName == "Pending").FirstOrDefault(), vietNamTimeZoneNow);

                await _unitOfWork.ConferenceRepository.CreateConferenceAsync(toBeCreatedConference);
                await _unitOfWork.TechnicalConferenceDetailRepository.CreateTechnicalAsync(new TechnicalConferenceDetail
                {
                    ConferenceId = toBeCreatedConference.ConferenceId,
                    TargetAudience = request.targetAudienceTechnicalConference
                });
                
                await _unitOfWork.CommitAsync();
                return await GetConferenceBasicAsync(toBeCreatedConference.ConferenceId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<TechnicalConferenceBasicStepResponse> GetConferenceBasicAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");
            var technical = await _unitOfWork.TechnicalConferenceDetailRepository.GetByConferenceIdAsync(conferenceId);

            return new TechnicalConferenceBasicStepResponse
            {
                conferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                TotalSlot = conference.TotalSlot,
                AvailableSlot = conference.AvailableSlot,
                Address = conference.Address,
                bannerImageFileUrl = conference.BannerImageUrl,
                createdAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                createdby = conference.CreatedBy,
                CityId = conference.CityId,
                ConferenceCategoryId = conference.ConferenceCategoryId,
                TicketSaleStart = conference.TicketSaleStart,
                TicketSaleEnd = conference.TicketSaleEnd,
                TargetAudience = technical?.TargetAudience
            };
        }

        public async Task<TechnicalConferenceBasicStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate;  // Fixed nullable DateOnly
            conference.EndDate = request.EndDate;         // Fixed nullable DateOnly
            conference.TotalSlot = request.TotalSlot ?? conference.TotalSlot;
            conference.AvailableSlot = request.TotalSlot ?? conference.AvailableSlot; // Update available slot if total is changed
            conference.Address = request.Address ?? conference.Address;
            conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
            conference.ConferenceCategoryId = request.ConferenceCategoryId ?? conference.ConferenceCategoryId;
            conference.CityId = request.CityId ?? conference.CityId;
            conference.TicketSaleStart = request.TicketSaleStart ?? conference.TicketSaleStart;
            conference.TicketSaleEnd = request.TicketSaleEnd ?? conference.TicketSaleEnd;

            if (request.BannerImageFile != null)
            {
                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                conference.BannerImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            }

            await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            return await GetConferenceBasicAsync(conferenceId);
        }

        #endregion

        #region Step 2: Prices

        public async Task<List<ConferencePriceWithPhasesResponse>> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create the conference price
                var conferencePriceRequest = request.TypeOfTicket;
                var conferencePrice = conferencePriceRequest.ToModel(conferenceId);
                // For technical conference, isAuthor must be false
                conferencePrice.IsAuthor = false;
                await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(conferencePrice);

                // Create price phases if provided
                var pricePhases = new List<PricePhase>();
                if (request.Phases != null)
                {
                    foreach (var phase in request.Phases)
                    {
                        var pricePhase = phase.ToModel(conferencePrice.ConferencePriceId);
                        await _unitOfWork.PricePhaseRepository.CreatePricePhaseAsync(pricePhase);
                        pricePhases.Add(pricePhase);
                    }
                }

                await _unitOfWork.CommitAsync();

                // Return the created price with its phases
                return new List<ConferencePriceWithPhasesResponse>
                {
                    new ConferencePriceWithPhasesResponse
                    {
                        ConferencePriceId = conferencePrice.ConferencePriceId,
                        TicketPrice = conferencePrice.TicketPrice,
                        TicketName = conferencePrice.TicketName,
                        TicketDescription = conferencePrice.TicketDescription,
                        PricePhases = pricePhases.Select(p => p.ToResponse()).ToList()
                    }
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ConferencePriceWithPhasesResponse>> GetConferencePricesAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            var responses = new List<ConferencePriceWithPhasesResponse>();

            foreach (var price in prices)
            {
                var phases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(price.ConferencePriceId);
                responses.Add(price.ToResponseWithPhases(phases));
            }

            return responses;
        }

        public async Task<ConferencePriceWithPhasesResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");

            if (request.TicketPrice.HasValue) price.TicketPrice = request.TicketPrice.Value;
            if (!string.IsNullOrEmpty(request.TicketName)) price.TicketName = request.TicketName;
            if (!string.IsNullOrEmpty(request.TicketDescription)) price.TicketDescription = request.TicketDescription;
            if (request.TotalSlot.HasValue)
            {
                price.TotalSlot = request.TotalSlot.Value;
                price.AvailableSlot = request.TotalSlot.Value; // Reset available slot when total is updated
            }

            await _unitOfWork.ConferencePriceRepository.UpdateConferencePriceAsync(price);

            var phases = await _unitOfWork.PricePhaseRepository.GetPricePhasesByConferencePriceIdAsync(price.ConferencePriceId);
            return price.ToResponseWithPhases(phases);
        }

        public async Task<bool> DeleteConferencePriceAsync(string priceId)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");

            // Check if there are any tickets already sold for this price
            var ticketCount = await _unitOfWork.TicketRepository.GetTicketCountByConferencePriceIdAsync(priceId);
            if (ticketCount > 0) throw new BadRequestException("Cannot delete price because tickets have already been sold for this price");

            return await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price) > 0;
        }

        #endregion

        #region Step 3: Sessions

        public async Task<List<ConferenceSessionWithMediaResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferenceSessionWithMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sessions != null)
                {
                    foreach (var session in request.Sessions)
                    {
                        if (session.RoomId == null || session.StartTime == null || session.EndTime == null || session.Date == null) 
                            throw new BadRequestException("Session must have a RoomId, StartTime, EndTime, and Date.");
                        
                        if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId) == null) 
                            throw new NotFoundException($"Room with ID {session.RoomId} not found");

                        // Validate session time availability
                        var startDateTime = new DateTime(session.Date.Value.Year, session.Date.Value.Month, session.Date.Value.Day);
                        var endDateTime = new DateTime(session.Date.Value.Year, session.Date.Value.Month, session.Date.Value.Day);
                        
                        startDateTime = startDateTime.AddHours(session.StartTime.Value.Hour).AddMinutes(session.StartTime.Value.Minute);
                        endDateTime = endDateTime.AddHours(session.EndTime.Value.Hour).AddMinutes(session.EndTime.Value.Minute);

                        await ValidateSessionTimeAvailability(startDateTime, endDateTime, session.RoomId);

                        var conferenceSession = session.ToModel(conferenceId);
                        await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                        // Add speakers for the session
                        if (session.Speaker != null)
                        {
                            foreach (var speakerRequest in session.Speaker)
                            {
                                var speaker = speakerRequest.ToModel(conferenceSession.ConferenceSessionId);
                                
                                if (speakerRequest.Image != null)
                                {
                                    using var stream = speakerRequest.Image.OpenReadStream();
                                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(speakerRequest.Image.FileName);
                                    speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, speakerRequest.Image.ContentType);
                                    speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
                                }

                                await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
                            }
                        }

                        // Add media for the session
                        if (session.SessionMedias != null)
                        {
                            foreach (var mediaRequest in session.SessionMedias)
                            {
                                var sessionMedia = mediaRequest.ToModel(conferenceSession.ConferenceSessionId);
                                
                                if (mediaRequest.MediaFile != null)
                                {
                                    using var stream = mediaRequest.MediaFile.OpenReadStream();
                                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(mediaRequest.MediaFile.FileName);
                                    sessionMedia.MediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencesessionmedia.ToString(), uniqueFileName, stream, mediaRequest.MediaFile.ContentType);
                                    sessionMedia.MediaUrl = _objectStorageSettings.EndPoint + sessionMedia.MediaUrl;
                                }

                                await _unitOfWork.ConferenceSessionMediumRepository.CreateConferenceSessionMediumAsync(sessionMedia);
                            }
                        }

                        // Get updated session with all details
                        var createdSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(conferenceSession.ConferenceSessionId);
                        responses.Add(createdSession.ToResponseWithMedia());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<ConferenceSessionWithMediaResponse>> GetConferenceSessionsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
            var responses = new List<ConferenceSessionWithMediaResponse>();

            foreach (var session in sessions)
            {
                responses.Add(session.ToResponseWithMedia());
            }

            return responses;
        }

        public async Task<ConferenceSessionWithMediaResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            var newStartTime = request.StartTime ?? TimeOnly.FromDateTime( session.StartTime.Value);
            var newEndTime = request.EndTime ?? TimeOnly.FromDateTime( session.EndTime.Value);
            var newDate = request.Date ?? session.SessionDate;
            var newRoomId = request.RoomId ?? session.RoomId;

            if (newStartTime == null || newEndTime == null || newDate == null || newRoomId == null) 
                throw new BadRequestException("Session must have a RoomId, StartTime, EndTime, and Date.");

            // Validate session time availability
            var startDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);
            var endDateTime = new DateTime(newDate.Value.Year, newDate.Value.Month, newDate.Value.Day);
            
            startDateTime = startDateTime.AddHours(newStartTime.Hour).AddMinutes(newStartTime.Minute);
            endDateTime = endDateTime.AddHours(newEndTime.Hour).AddMinutes(newEndTime.Minute);

            await ValidateSessionTimeAvailability(startDateTime, endDateTime, newRoomId, sessionId);

            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;
            session.StartTime = startDateTime;
            session.EndTime = endDateTime;
            session.SessionDate = newDate;
            session.RoomId = newRoomId;

            await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);

            var updatedSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            return updatedSession.ToResponseWithMedia();
        }

        public async Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Session with ID {sessionId} not found.");

            // Find an existing speaker for this session (if any)
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerBySessionIdAsync(sessionId);
            if (speaker == null)
            {
                // Create a new speaker
                speaker = new Speaker
                {
                    SpeakerId = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    ConferenceSessionId = sessionId
                };
                
                if (request.Image != null)
                {
                    using var stream = request.Image.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                    speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                    speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
                }
                
                await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
            }
            else
            {
                // Update existing speaker
                speaker.Name = request.Name ?? speaker.Name;
                speaker.Description = request.Description ?? speaker.Description;

                if (request.Image != null)
                {
                    using var stream = request.Image.OpenReadStream();
                    var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.Image.FileName);
                    speaker.Image = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.speakerimage.ToString(), uniqueFileName, stream, request.Image.ContentType);
                    speaker.Image = _objectStorageSettings.EndPoint + speaker.Image;
                }
                
                await _unitOfWork.SpeakerRepository.UpdateSpeakerAsync(speaker);
            }

            return speaker.ToResponse();
        }

        public async Task<bool> DeleteConferenceSessionAsync(string sessionId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            // Delete all speakers associated with this session
            var speakers = await _unitOfWork.SpeakerRepository.GetSpeakersBySessionIdAsync(sessionId);
            foreach (var speaker in speakers)
            {
                await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker);
            }

            // Delete all media associated with this session
            var mediaList = await _unitOfWork.ConferenceSessionMediumRepository.GetMediaBySessionIdAsync(sessionId);
            foreach (var media in mediaList)
            {
                await _unitOfWork.ConferenceSessionMediumRepository.DeleteConferenceSessionMediumAsync(media);
            }

            return await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session) > 0;
        }

        #endregion

        #region Step 4: Policies

        public async Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferencePolicyResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Policies != null)
                {
                    foreach (var policy in request.Policies)
                    {
                        var conferencePolicy = policy.ToModel(conferenceId);
                        await _unitOfWork.ConferencePolicyRepository.CreateConferencePolicyAsync(conferencePolicy);
                        responses.Add(conferencePolicy.ToResponse());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
            return policies.Select(p => p.ToResponse()).ToList();
        }

        public async Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            if (!string.IsNullOrEmpty(request.PolicyName)) policy.PolicyName = request.PolicyName;
            if (!string.IsNullOrEmpty(request.Description)) policy.Description = request.Description;

            await _unitOfWork.ConferencePolicyRepository.UpdateConferencePolicyAsync(policy);
            return policy.ToResponse();
        }

        public async Task<bool> DeleteConferencePolicyAsync(string policyId)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            return await _unitOfWork.ConferencePolicyRepository.DeleteConferencePolicyAsync(policy) > 0;
        }

        #endregion

        #region Step 5: Media

        public async Task<List<ConferenceMediaResponse>> AddConferenceMediaAsync(string conferenceId, AddConferenceMediaRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferenceMediaResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var media in request.Media)
                {
                    string? mediaUrl = media.MediaUrl;
                    if (media.MediaFile != null)
                    {
                        //if (await _unitOfWork.MediaTypeRepository.GetMediaTypeByIdAsync(media.MediaTypeId) == null) throw new NotFoundException($"Media type with ID {media.MediaTypeId} not found");
                        using var stream = media.MediaFile.OpenReadStream();
                        var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(media.MediaFile.FileName);
                        mediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, media.MediaFile.ContentType);
                    }
                    var conferenceMedia = new ConferenceMedium { ConferenceMediaId = Guid.NewGuid().ToString(), ConferenceMediaUrl = mediaUrl, ConferenceId = conferenceId, };
                    await _unitOfWork.ConferenceMediaRepository.CreateConferenceMediaAsync(conferenceMedia);
                    responses.Add(new ConferenceMediaResponse { MediaId = conferenceMedia.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(conferenceMedia.ConferenceMediaUrl)});
                }
                await _unitOfWork.CommitAsync();
            }catch (Exception e)
            {
                await _unitOfWork.RollbackAsync();
            }
            return responses;
        }

        public async Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conferenceId);
            return media.Select(m => new ConferenceMediaResponse { MediaId = m.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(m.ConferenceMediaUrl)}).ToList();
        }

        public async Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");

            if (request.MediaFile != null)
            {
                using var stream = request.MediaFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.MediaFile.FileName);
                media.ConferenceMediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, request.MediaFile.ContentType);
                media.ConferenceMediaUrl = _objectStorageSettings.EndPoint + media.ConferenceMediaUrl;
            }
            else if (!string.IsNullOrEmpty(request.MediaUrl))
            {
                media.ConferenceMediaUrl = request.MediaUrl;
            }

            //media.MediaTypeId = request.MediaTypeId ?? media.MediaTypeId;
            await _unitOfWork.ConferenceMediaRepository.UpdateConferenceMediaAsync(media);
            return new ConferenceMediaResponse { MediaId = media.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(media.ConferenceMediaUrl) };
        }

        public async Task<bool> DeleteConferenceMediaAsync(string mediaId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");
            return await _unitOfWork.ConferenceMediaRepository.DeleteConferenceMediaAsync(media) > 0;
        }

        #endregion

        #region Step 6: Sponsors

        public async Task<List<SponsorResponse>> AddConferenceSponsorsAsync(string conferenceId, AddConferenceSponsorsRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<SponsorResponse>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Sponsors != null)
                {
                    foreach (var sponsor in request.Sponsors)
                    {
                        string? imageUrl = sponsor.ImageUrl;
                        if (sponsor.ImageFile != null)
                        {
                            using var stream = sponsor.ImageFile.OpenReadStream();
                            var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(sponsor.ImageFile.FileName);
                            imageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, sponsor.ImageFile.ContentType);
                            imageUrl = _objectStorageSettings.EndPoint + imageUrl;
                        }

                        var conferenceSponsor = sponsor.ToModel(conferenceId);
                        conferenceSponsor.ImageUrl = imageUrl;
                        
                        await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
                        responses.Add(conferenceSponsor.ToResponse());
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return responses;
        }

        public async Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
            return sponsors.Select(s => s.ToResponse()).ToList();
        }

        public async Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            if (!string.IsNullOrEmpty(request.Name)) sponsor.Name = request.Name;

            if (request.ImageFile != null)
            {
                using var stream = request.ImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ImageFile.FileName);
                sponsor.ImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, request.ImageFile.ContentType);
                sponsor.ImageUrl = _objectStorageSettings.EndPoint + sponsor.ImageUrl;
            }
            else if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                sponsor.ImageUrl = request.ImageUrl;
            }

            await _unitOfWork.SponsorRepository.UpdateSponsorAsync(sponsor);
            return sponsor.ToResponse();
        }

        public async Task<bool> DeleteSponsorAsync(string sponsorId)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            return await _unitOfWork.SponsorRepository.DeleteSponsorAsync(sponsor) > 0;
        }

        #endregion
    }
}