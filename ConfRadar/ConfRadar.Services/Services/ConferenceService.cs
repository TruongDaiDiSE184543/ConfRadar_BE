using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.DTOs.Conference;
using System.Text.Json;
using System.Transactions;

namespace ConfRadar.Services.Services
{
    public interface IConferenceService
    {
        Task<string> CreateConferenceAsync(CreateConferenceRequest request, string userId);
        Task<int> UpdateConferenceAsync(UpdateConferenceRequest request, string conferenceId);
        Task<int> DeleteConferenceAsync(string conferenceId);
        Task<ConferenceResponse> GetConferenceByIdAsync(string conferenceId);
        Task<List<ConferenceResponse>> GetAllConferencesAsync();
    }

    public class ConferenceService : IConferenceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConferenceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateConferenceAsync(CreateConferenceRequest request, string userId)
        {
            // Check if user exists
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Check if user has organizer role
            var userRoles = user.UserRoles;
            var isOrganizer = userRoles.Any(ur => ur.Role.RoleName == "Organizer");
            if (!isOrganizer)
            {
                throw new ConfRadarAuthenticationException("Only users with organizer role can create conferences");
            }

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

            // Create the conference
            var conference = new Conference
            {
                ConferenceId = conferenceId,
                ConferenceName = request.ConferenceName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Capacity = request.Capacity,
                Address = request.Address,
                BannerImageUrl = request.BannerImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsInternalHosted = request.IsInternalHosted,
                IsResearchConference = request.IsResearchConference,
                IsActive = true,
                ConferenceCategoryId = category.ConferenceCategoryId,
                UserId = userId,
                GlobalStatusId = request.GlobalStatusId // You might want to set this to a default "Active" status
            };

            var result = await _unitOfWork.ConferenceRepository.CreateConferenceAsync(conference);
            
            if (result <= 0)
            {
                throw new BadRequestException("Failed to create conference");
            }

            // Create conference policies if provided
            if (request.Policies != null && request.Policies.Any())
            {
                foreach (var policy in request.Policies)
                {
                    var conferencePolicy = new ConferencePolicy
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
                    var conferenceMedia = new ConferenceMedium
                    {
                        ConferenceMediaId = Guid.NewGuid().ToString(),
                        ConferenceMediaUrl = media.MediaUrl,
                        ConferenceId = conferenceId,
                        MediaTypeId = media.MediaTypeId
                    };
                    await _unitOfWork.ConferenceMediumRepository.CreateConferenceMediumAsync(conferenceMedia);
                }
            }

            // Create sponsors if provided
            if (request.Sponsors != null && request.Sponsors.Any())
            {
                foreach (var sponsor in request.Sponsors)
                {
                    var conferenceSponsor = new Sponsor
                    {
                        SponsorId = Guid.NewGuid().ToString(),
                        Name = sponsor.Name,
                        ImageUrl = sponsor.ImageUrl,
                        ConferenceId = conferenceId
                    };
                    await _unitOfWork.SponsorRepository.CreateSponsorAsync(conferenceSponsor);
                }
            }

            // Create prices if provided
            if (request.Prices != null && request.Prices.Any())
            {
                foreach (var price in request.Prices)
                {
                    var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByIdAsync(price.PricePhaseId);
                    if (pricePhase == null)
                    {
                        throw new NotFoundException($"Price phase with ID {price.PricePhaseId} not found");
                    }

                    var conferencePrice = new ConferencePrice
                    {
                        ConferencePriceId = Guid.NewGuid().ToString(),
                        TicketPrice = price.TicketPrice,
                        TicketName = price.TicketName,
                        TicketDescription = price.TicketDescription,
                        ActualPrice = price.ActualPrice,
                        PricePhaseId = price.PricePhaseId,
                        ConferenceId = conferenceId
                    };
                    await _unitOfWork.ConferencePriceRepository.CreateConferencePriceAsync(conferencePrice);
                }
            }

            // Create sessions if provided
            if (request.Sessions != null && request.Sessions.Any())
            {
                foreach (var session in request.Sessions)
                {
                    var conferenceSession = new ConferenceSession
                    {
                        ConferenceSessionId = Guid.NewGuid().ToString(),
                        Title = session.Title,
                        Description = session.Description,
                        StartTime = session.StartTime,
                        EndTime = session.EndTime,
                        Date = session.Date,
                        ConferenceId = conferenceId,
                        StatusId = session.StatusId,
                        RoomId = session.RoomId
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

            // Create destination and rooms if provided
            if (request.Destination != null)
            {
                var destination = new Destination
                {
                    DestinationId = Guid.NewGuid().ToString(),
                    Name = request.Destination.Name,
                    City = request.Destination.City,
                    District = request.Destination.District,
                    Street = request.Destination.Street
                };
                await _unitOfWork.DestinationRepository.CreateDestinationAsync(destination);

                // Update conference location
                conference.LocationId = destination.DestinationId;
                await _unitOfWork.ConferenceRepository.UpdateConferenceAsync(conference);
            }

            return conferenceId;
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
            existingConference.Capacity = request.Capacity ?? existingConference.Capacity;
            existingConference.Address = request.Address ?? existingConference.Address;
            existingConference.BannerImageUrl = request.BannerImageUrl ?? existingConference.BannerImageUrl;
            existingConference.IsInternalHosted = request.IsInternalHosted ?? existingConference.IsInternalHosted;
            existingConference.IsResearchConference = request.IsResearchConference ?? existingConference.IsResearchConference;
            existingConference.IsActive = request.IsActive ?? existingConference.IsActive;

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
                Capacity = conference.Capacity,
                Address = conference.Address,
                BannerImageUrl = conference.BannerImageUrl,
                CreatedAt = conference.CreatedAt,
                IsInternalHosted = conference.IsInternalHosted,
                IsResearchConference = conference.IsResearchConference,
                IsActive = conference.IsActive,
                UserId = conference.UserId,
                LocationId = conference.LocationId,
                CategoryId = conference.ConferenceCategoryId,
                Policies = conference.ConferencePolicies?.Select(p => new ConferencePolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    Description = p.Description
                }).ToList(),
                Media = conference.ConferenceMedia?.Select(m => new ConferenceMediaResponse
                {
                    MediaId = m.ConferenceMediaId,
                    MediaUrl = m.ConferenceMediaUrl,
                    MediaTypeId = m.MediaTypeId
                }).ToList(),
                Sponsors = conference.Sponsors?.Select(s => new SponsorResponse
                {
                    SponsorId = s.SponsorId,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl
                }).ToList(),
                Prices = conference.ConferencePrices?.Select(p => new ConferencePriceResponse
                {
                    PriceId = p.ConferencePriceId,
                    TicketPrice = p.TicketPrice,
                    TicketName = p.TicketName,
                    TicketDescription = p.TicketDescription,
                    ActualPrice = p.ActualPrice,
                    PricePhaseId = p.PricePhaseId
                }).ToList(),
                Sessions = conference.ConferenceSessions?.Select(s => new ConferenceSessionResponse
                {
                    SessionId = s.ConferenceSessionId,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Date = s.Date,
                    ConferenceId = s.ConferenceId,
                    StatusId = s.StatusId,
                    RoomId = s.RoomId,
                    Speaker = s.Speaker != null ? new SpeakerResponse
                    {
                        Name = s.Speaker.Name,
                        Description = s.Speaker.Description
                    } : null
                }).ToList()
            };

            return response;
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
                    Capacity = conference.Capacity,
                    Address = conference.Address,
                    BannerImageUrl = conference.BannerImageUrl,
                    CreatedAt = conference.CreatedAt,
                    IsInternalHosted = conference.IsInternalHosted,
                    IsResearchConference = conference.IsResearchConference,
                    IsActive = conference.IsActive,
                    UserId = conference.UserId,
                    LocationId = conference.LocationId,
                    CategoryId = conference.ConferenceCategoryId
                };
                responses.Add(response);
            }

            return responses;
        }
    }
}