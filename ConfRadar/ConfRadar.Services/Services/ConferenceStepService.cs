using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStepService
    {
        // Step 1: Basic Conference Creation
        Task<ConferenceStepResponse> CreateConferenceBasicAsync(CreateConferenceBasicRequest request, string userId);
        Task<ConferenceStepResponse> GetConferenceBasicAsync(string conferenceId);
        Task<ConferenceStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request);

        // Step 2: Add Conference Prices
        Task<List<ConferencePriceStepResponse>> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request);
        Task<List<ConferencePriceStepResponse>> GetConferencePricesAsync(string conferenceId);
        Task<ConferencePriceStepResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request);
        Task<bool> DeleteConferencePriceAsync(string priceId);

        // Step 3: Add Conference Sessions
        Task<List<ConferenceSessionStepResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request);
        Task<List<ConferenceSessionStepResponse>> GetConferenceSessionsAsync(string conferenceId);
        Task<ConferenceSessionStepResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request);
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
            var startTimeUtc = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

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

                var existingStartUtc = DateTime.SpecifyKind(existingSession.StartTime.Value, DateTimeKind.Utc);
                var existingEndUtc = DateTime.SpecifyKind(existingSession.EndTime.Value, DateTimeKind.Utc);

                if (startTimeUtc < existingEndUtc && endTimeUtc > existingStartUtc)
                {
                    throw new BadRequestException($"Session conflicts with an existing session in room {roomId} from {existingStartUtc:HH:mm} to {existingEndUtc:HH:mm}.");
                }
            }
        }

        #endregion

        #region Step 1: Basic Conference

        public async Task<ConferenceStepResponse> CreateConferenceBasicAsync(CreateConferenceBasicRequest request, string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null || !user.UserRoles.Any(ur => ur.Role.RoleName == "Conference Organizer"))
            {
                throw new ConfRadarAuthenticationException("User not found or is not a Conference Organizer.");
            }

            var category = await _unitOfWork.ConferenceCategoryRepository.GetCategoryByCategoryName(request.CategoryName);
            if (category == null)
            {
                category = new ConferenceCategory { ConferenceCategoryId = Guid.NewGuid().ToString(), ConferenceCategoryName = request.CategoryName };
                await _unitOfWork.ConferenceCategoryRepository.CreateConferenceCategoryAsync(category);
            }

            string? bannerImageUrl = null;
            if (request.BannerImageFile != null)
            {
                using var stream = request.BannerImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.BannerImageFile.FileName);
                bannerImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencebanner.ToString(), uniqueFileName, stream, request.BannerImageFile.ContentType);
            }

            var conference = new Conference
            {
                ConferenceId = Guid.NewGuid().ToString(),
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
                //LocationId = request.LocationId,
                //GlobalStatusId = request.GlobalStatusId
            };

            await _unitOfWork.ConferenceRepository.CreateConferenceAsync(conference);
            return await GetConferenceBasicAsync(conference.ConferenceId);
        }

        public async Task<ConferenceStepResponse> GetConferenceBasicAsync(string conferenceId)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            return new ConferenceStepResponse
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
                CategoryId = conference.ConferenceCategoryId
            };
        }

        public async Task<ConferenceStepResponse> UpdateConferenceBasicAsync(string conferenceId, UpdateConferenceBasicRequest request)
        {
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (conference == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            conference.ConferenceName = request.ConferenceName ?? conference.ConferenceName;
            conference.Description = request.Description ?? conference.Description;
            conference.StartDate = request.StartDate ?? conference.StartDate;
            conference.EndDate = request.EndDate ?? conference.EndDate;
            //conference.Capacity = request.Capacity ?? conference.Capacity;
            conference.Address = request.Address ?? conference.Address;
            conference.IsInternalHosted = request.IsInternalHosted ?? conference.IsInternalHosted;
            conference.IsResearchConference = request.IsResearchConference ?? conference.IsResearchConference;
            //conference.IsActive = request.IsActive ?? conference.IsActive;
            //conference.LocationId = request.LocationId ?? conference.LocationId;
            //conference.GlobalStatusId = request.GlobalStatusId ?? conference.GlobalStatusId;

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

        public async Task<List<ConferencePriceStepResponse>> AddConferencePricesAsync(string conferenceId, AddConferencePricesRequest request)
        {
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferencePriceStepResponse>();
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

            if (request.Prices != null)
            {
                foreach (var price in request.Prices)
                {
                    var conferencePrice = new ConferencePrice
                    {
                        ConferencePriceId = Guid.NewGuid().ToString(),
                        TicketPrice = price.TicketPrice,
                        TicketName = price.TicketName,
                        TicketDescription = price.TicketDescription,
                        //ActualPrice = price.ActualPrice,
                        //PricePhaseId = pricePhaseId,
                        ConferenceId = conferenceId
                    };
                    await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(conferencePrice);

                    responses.Add(new ConferencePriceStepResponse
                    {
                        PriceId = conferencePrice.ConferencePriceId,
                        TicketPrice = conferencePrice.TicketPrice,
                        TicketName = conferencePrice.TicketName,
                        TicketDescription = conferencePrice.TicketDescription,
                        //ActualPrice = conferencePrice.ActualPrice,
                        CurrentPhase = "Standard", // TODO: Implement dynamic phase calculation
                        //PricePhaseId = conferencePrice.PricePhaseId
                    });
                }
            }
            return responses;
        }

        public async Task<List<ConferencePriceStepResponse>> GetConferencePricesAsync(string conferenceId)
        {
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var prices = await _unitOfWork.ConferencePriceRepository.GetPricesByConferenceIdAsync(conferenceId);
            return prices.Select(p => new ConferencePriceStepResponse
            {
                PriceId = p.ConferencePriceId,
                TicketPrice = p.TicketPrice,
                TicketName = p.TicketName,
                TicketDescription = p.TicketDescription,
                //ActualPrice = p.ActualPrice,
                CurrentPhase = "Standard", // TODO: Implement dynamic phase calculation
                //PricePhaseId = p.PricePhaseId
            }).ToList();
        }

        public async Task<ConferencePriceStepResponse> UpdateConferencePriceAsync(string priceId, UpdateConferencePriceRequest request)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");

            price.TicketPrice = request.TicketPrice ?? price.TicketPrice;
            price.TicketName = request.TicketName ?? price.TicketName;
            price.TicketDescription = request.TicketDescription ?? price.TicketDescription;
            //price.ActualPrice = request.ActualPrice ?? price.ActualPrice;
            //price.PricePhaseId = request.PricePhaseId ?? price.PricePhaseId;

            await _unitOfWork.ConferencePriceRepository.UpdateConferencePriceAsync(price);
            return new ConferencePriceStepResponse { PriceId = price.ConferencePriceId, TicketPrice = price.TicketPrice, TicketName = price.TicketName, TicketDescription = price.TicketDescription, ActualPrice = price.TicketPrice, CurrentPhase = "Standard", /*PricePhaseId = price.PricePhases */};
        }

        public async Task<bool> DeleteConferencePriceAsync(string priceId)
        {
            var price = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(priceId);
            if (price == null) throw new NotFoundException($"Conference price with ID {priceId} not found");
            return await _unitOfWork.ConferencePriceRepository.DeleteConferencePriceAsync(price) > 0;
        }

        #endregion

        #region Step 3: Sessions

        public async Task<List<ConferenceSessionStepResponse>> AddConferenceSessionsAsync(string conferenceId, AddConferenceSessionsRequest request)
        {
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferenceSessionStepResponse>();
            if (request.Sessions != null)
            {
                foreach (var session in request.Sessions)
                {
                    if (session.RoomId == null || session.StartTime == null || session.EndTime == null) throw new BadRequestException("Session must have a RoomId, StartTime, and EndTime.");
                    if (await _unitOfWork.RoomRepository.GetRoomByIdAsync(session.RoomId) == null) throw new NotFoundException($"Room with ID {session.RoomId} not found");

                    await ValidateSessionTimeAvailability(session.StartTime.Value, session.EndTime.Value, session.RoomId);

                    var conferenceSession = new ConferenceSession
                    {
                        ConferenceSessionId = Guid.NewGuid().ToString(),
                        Title = session.Title,
                        Description = session.Description,
                        StartTime = session.StartTime,
                        EndTime = session.EndTime,
                        ConferenceId = conferenceId,
                        RoomId = session.RoomId
                    };
                    await _unitOfWork.ConferenceSessionRepository.CreateConferenceSessionAsync(conferenceSession);

                    SpeakerResponse? speakerResponse = null;
                    if (session.Speaker != null)
                    {
                        var speaker = new Speaker { ConferenceSessionId = conferenceSession.ConferenceSessionId, Name = session.Speaker.Name, Description = session.Speaker.Description };
                        await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(speaker);
                        speakerResponse = new SpeakerResponse { Name = speaker.Name, Description = speaker.Description };
                    }

                    responses.Add(new ConferenceSessionStepResponse
                    {
                        SessionId = conferenceSession.ConferenceSessionId,
                        Title = conferenceSession.Title,
                        Description = conferenceSession.Description,
                        StartTime = conferenceSession.StartTime,
                        EndTime = conferenceSession.EndTime,
                        ConferenceId = conferenceSession.ConferenceId,
                        RoomId = conferenceSession.RoomId,
                        Speaker = speakerResponse
                    });
                }
            }
            return responses;
        }

        public async Task<List<ConferenceSessionStepResponse>> GetConferenceSessionsAsync(string conferenceId)
        {
            var sessions = await _unitOfWork.ConferenceSessionRepository.GetSessionsByConferenceIdAsync(conferenceId);
            return sessions.Select(s => new ConferenceSessionStepResponse
            {
                SessionId = s.ConferenceSessionId,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                ConferenceId = s.ConferenceId,
                RoomId = s.RoomId,
                Room = s.Room != null ? new RoomInfoResponse { RoomId = s.Room.RoomId, Number = s.Room.Number, DisplayName = s.Room.DisplayName, DestinationId = s.Room.DestinationId } : null,
                //Speaker = s.Speaker != null ? new SpeakerResponse { Name = s.Speakers.Name, Description = s.Speakers.Description } : null
            }).ToList();
        }

        public async Task<ConferenceSessionStepResponse> UpdateConferenceSessionAsync(string sessionId, UpdateConferenceSessionRequest request)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            var newStartTime = request.StartTime ?? session.StartTime;
            var newEndTime = request.EndTime ?? session.EndTime;
            var newRoomId = request.RoomId ?? session.RoomId;

            if (newStartTime == null || newEndTime == null || newRoomId == null) throw new BadRequestException("Session must have a RoomId, StartTime, and EndTime.");

            await ValidateSessionTimeAvailability(newStartTime.Value, newEndTime.Value, newRoomId, sessionId);

            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;
            session.StartTime = newStartTime;
            session.EndTime = newEndTime;
            session.RoomId = newRoomId;

            await _unitOfWork.ConferenceSessionRepository.UpdateConferenceSessionAsync(session);

            var updatedSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(sessionId);
            return new ConferenceSessionStepResponse
            {
                SessionId = updatedSession.ConferenceSessionId,
                Title = updatedSession.Title,
                Description = updatedSession.Description,
                StartTime = updatedSession.StartTime,
                EndTime = updatedSession.EndTime,
                ConferenceId = updatedSession.ConferenceId,
                RoomId = updatedSession.RoomId,
                Room = updatedSession.Room != null ? new RoomInfoResponse { RoomId = updatedSession.Room.RoomId, DisplayName = updatedSession.Room.DisplayName } : null,
                //Speaker = updatedSession.Speaker != null ? new SpeakerResponse { Name = updatedSession.Speaker.Name, Description = updatedSession.Speaker.Description } : null
            };
        }

        public async Task<SpeakerResponse> UpdateSpeakerAsync(string sessionId, UpdateSpeakerRequest request)
        {
            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(sessionId);
            if (speaker == null)
            {
                if (await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId) == null) throw new NotFoundException($"Session with ID {sessionId} not found.");

                var newSpeaker = new Speaker { ConferenceSessionId = sessionId, Name = request.Name, Description = request.Description };
                await _unitOfWork.SpeakerRepository.CreateSpeakerAsync(newSpeaker);
                return new SpeakerResponse { Name = newSpeaker.Name, Description = newSpeaker.Description };
            }

            speaker.Name = request.Name ?? speaker.Name;
            speaker.Description = request.Description ?? speaker.Description;
            await _unitOfWork.SpeakerRepository.UpdateSpeakerAsync(speaker);
            return new SpeakerResponse { Name = speaker.Name, Description = speaker.Description };
        }

        public async Task<bool> DeleteConferenceSessionAsync(string sessionId)
        {
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Conference session with ID {sessionId} not found");

            var speaker = await _unitOfWork.SpeakerRepository.GetSpeakerByIdAsync(sessionId);
            if (speaker != null) await _unitOfWork.SpeakerRepository.DeleteSpeakerAsync(speaker);

            return await _unitOfWork.ConferenceSessionRepository.DeleteConferenceSessionAsync(session) > 0;
        }

        #endregion

        #region Step 4: Policies

        public async Task<List<ConferencePolicyResponse>> AddConferencePoliciesAsync(string conferenceId, AddConferencePoliciesRequest request)
        {
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferencePolicyResponse>();
            if (request.Policies != null)
            {
                foreach (var policy in request.Policies)
                {
                    var conferencePolicy = new Policy { PolicyId = Guid.NewGuid().ToString(), PolicyName = policy.PolicyName, Description = policy.Description, ConferenceId = conferenceId };
                    await _unitOfWork.ConferencePolicyRepository.CreateConferencePolicyAsync(conferencePolicy);
                    responses.Add(new ConferencePolicyResponse { PolicyId = conferencePolicy.PolicyId, PolicyName = conferencePolicy.PolicyName, Description = conferencePolicy.Description });
                }
            }
            return responses;
        }

        public async Task<List<ConferencePolicyResponse>> GetConferencePoliciesAsync(string conferenceId)
        {
            var policies = await _unitOfWork.ConferencePolicyRepository.GetPoliciesByConferenceIdAsync(conferenceId);
            return policies.Select(p => new ConferencePolicyResponse { PolicyId = p.PolicyId, PolicyName = p.PolicyName, Description = p.Description }).ToList();
        }

        public async Task<ConferencePolicyResponse> UpdateConferencePolicyAsync(string policyId, UpdateConferencePolicyRequest request)
        {
            var policy = await _unitOfWork.ConferencePolicyRepository.GetConferencePolicyByIdAsync(policyId);
            if (policy == null) throw new NotFoundException($"Conference policy with ID {policyId} not found");

            policy.PolicyName = request.PolicyName ?? policy.PolicyName;
            policy.Description = request.Description ?? policy.Description;
            await _unitOfWork.ConferencePolicyRepository.UpdateConferencePolicyAsync(policy);
            return new ConferencePolicyResponse { PolicyId = policy.PolicyId, PolicyName = policy.PolicyName, Description = policy.Description };
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
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<ConferenceMediaResponse>();
            if (request.Media != null)
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
                    responses.Add(new ConferenceMediaResponse { MediaId = conferenceMedia.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(conferenceMedia.ConferenceMediaUrl) });
                }
            }
            return responses;
        }

        public async Task<List<ConferenceMediaResponse>> GetConferenceMediaAsync(string conferenceId)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetMediaByConferenceIdAsync(conferenceId);
            return media.Select(m => new ConferenceMediaResponse { MediaId = m.ConferenceMediaId, MediaUrl = AddBaseUrlToUrl(m.ConferenceMediaUrl) }).ToList();
        }

        public async Task<ConferenceMediaResponse> UpdateConferenceMediaAsync(string mediaId, UpdateConferenceMediaRequest request)
        {
            var media = await _unitOfWork.ConferenceMediaRepository.GetConferenceMediaByIdAsync(mediaId);
            if (media == null) throw new NotFoundException($"Conference media with ID {mediaId} not found");

            if (request.MediaFile != null)
            {
                //var mediaTypeId = request.MediaTypeId ?? media.MediaTypeId;
                //if (await _unitOfWork.MediaTypeRepository.GetMediaTypeByIdAsync(mediaTypeId) == null) throw new NotFoundException($"Media type with ID {mediaTypeId} not found");
                using var stream = request.MediaFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.MediaFile.FileName);
                media.ConferenceMediaUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, request.MediaFile.ContentType);
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
            if (await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId) == null) throw new NotFoundException($"Conference with ID {conferenceId} not found");

            var responses = new List<SponsorResponse>();
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
                    }
                    var conferenceSponsor = new Sponsor { SponsorId = Guid.NewGuid().ToString(), Name = sponsor.Name, ImageUrl = imageUrl, ConferenceId = conferenceId };
                    await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
                    responses.Add(new SponsorResponse { SponsorId = conferenceSponsor.SponsorId, Name = conferenceSponsor.Name, ImageUrl = AddBaseUrlToUrl(conferenceSponsor.ImageUrl) });
                }
            }
            return responses;
        }

        public async Task<List<SponsorResponse>> GetConferenceSponsorsAsync(string conferenceId)
        {
            var sponsors = await _unitOfWork.SponsorRepository.GetSponsorsByConferenceIdAsync(conferenceId);
            return sponsors.Select(s => new SponsorResponse { SponsorId = s.SponsorId, Name = s.Name, ImageUrl = AddBaseUrlToUrl(s.ImageUrl) }).ToList();
        }

        public async Task<SponsorResponse> UpdateSponsorAsync(string sponsorId, UpdateSponsorRequest request)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetSponsorByIdAsync(sponsorId);
            if (sponsor == null) throw new NotFoundException($"Conference sponsor with ID {sponsorId} not found");

            sponsor.Name = request.Name ?? sponsor.Name;

            if (request.ImageFile != null)
            {
                using var stream = request.ImageFile.OpenReadStream();
                var uniqueFileName = _tokenService.GenerateSecureRandomToken() + Path.GetExtension(request.ImageFile.FileName);
                sponsor.ImageUrl = await _objectStorageFileService.UploadFileAsync(ObjectStorageBucketEnum.conferencemedia.ToString(), uniqueFileName, stream, request.ImageFile.ContentType);
            }
            else if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                sponsor.ImageUrl = request.ImageUrl;
            }

            await _unitOfWork.SponsorRepository.UpdateSponsorAsync(sponsor);
            return new SponsorResponse { SponsorId = sponsor.SponsorId, Name = sponsor.Name, ImageUrl = AddBaseUrlToUrl(sponsor.ImageUrl) };
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