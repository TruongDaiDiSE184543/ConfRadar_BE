using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceStepMappers
    {
        // Conference Price Mappers
        public static ConferencePrice ToModel(this CreateConferencePriceRequest request, string conferenceId)
        {
            return new ConferencePrice
            {
                ConferencePriceId = Guid.NewGuid().ToString(),
                TicketPrice = request.TicketPrice,
                TicketName = request.TicketName,
                TicketDescription = request.TicketDescription,
                IsAuthor = request.isAuthor,
                TotalSlot = request.TotalSlot,
                AvailableSlot = request.TotalSlot, // Initialize available slot to total slot
                ConferenceId = conferenceId
            };
        }

        public static ConferencePriceStepResponse ToResponse(this ConferencePrice model)
        {
            return new ConferencePriceStepResponse
            {
                PriceId = model.ConferencePriceId,
                TicketPrice = model.TicketPrice,
                TicketName = model.TicketName,
                TicketDescription = model.TicketDescription,
                ActualPrice = model.TicketPrice, // Initially same as ticket price
                CurrentPhase = "Standard" // Default phase
            };
        }

        public static ConferencePriceWithPhasesResponse ToResponseWithPhases(this ConferencePrice model, List<PricePhase> phases)
        {
            return new ConferencePriceWithPhasesResponse
            {
                ConferencePriceId = model.ConferencePriceId,
                TicketPrice = model.TicketPrice,
                TicketName = model.TicketName,
                TicketDescription = model.TicketDescription,
                PricePhases = phases?.Select(p => p.ToResponse()).ToList()
            };
        }

        // Price Phase Mappers
        public static PricePhase ToModel(this CreatePricePhaseRequest request, string conferencePriceId)
        {
            return new PricePhase
            {
                PricePhaseId = Guid.NewGuid().ToString(),
                PhaseName = request.PhaseName,
                ApplyPercent = request.ApplyPercent,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSlot = request.Totalslot,
                AvailableSlot = request.Totalslot, // Initialize available slot to total slot
                ConferencePriceId = conferencePriceId
            };
        }

        public static PricePhaseResponse ToResponse(this PricePhase model)
        {
            return new PricePhaseResponse
            {
                PricePhaseId = model.PricePhaseId,
                PhaseName = model.PhaseName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ApplyPercent = model.ApplyPercent,
                TotalSlot = model.TotalSlot,
                AvailableSlot = model.AvailableSlot
            };
        }

        // Conference Session Mappers
        public static ConferenceSession ToModel(this CreateConferenceSessionRequest request, string conferenceId)
        {
            // Convert TimeOnly and DateOnly to DateTime for PostgreSQL timestamp
            var startDateTime = new DateTime(request.Date!.Value.Year, request.Date!.Value.Month, request.Date!.Value.Day);
            var endDateTime = new DateTime(request.Date!.Value.Year, request.Date!.Value.Month, request.Date!.Value.Day);
            
            startDateTime = startDateTime.AddHours(request.StartTime!.Value.Hour).AddMinutes(request.StartTime!.Value.Minute);
            endDateTime = endDateTime.AddHours(request.EndTime!.Value.Hour).AddMinutes(request.EndTime!.Value.Minute);

            return new ConferenceSession
            {
                ConferenceSessionId = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                StartTime = startDateTime, // Using DateTime for PostgreSQL timestamp
                EndTime = endDateTime,     // Using DateTime for PostgreSQL timestamp
                SessionDate = request.Date,
                ConferenceId = conferenceId,
                RoomId = request.RoomId
            };
        }

        public static ConferenceSessionStepResponse ToResponse(this ConferenceSession model)
        {
            // Convert DateTime to TimeOnly and DateOnly for the response
            TimeOnly? startTime = null;
            TimeOnly? endTime = null;
            DateOnly? date = null;

            if (model.StartTime.HasValue)
            {
                startTime = TimeOnly.FromDateTime(model.StartTime.Value);
                date = DateOnly.FromDateTime(model.StartTime.Value);
            }

            if (model.EndTime.HasValue)
            {
                endTime = TimeOnly.FromDateTime(model.EndTime.Value);
            }

            return new ConferenceSessionStepResponse
            {
                SessionId = model.ConferenceSessionId,
                Title = model.Title,
                Description = model.Description,
                StartTime = startTime,
                EndTime = endTime,
                Date = date,
                RoomId = model.RoomId,
                Speakers = model.Speakers?.Select(s => s.ToResponse()).ToList()
            };
        }

        public static ConferenceSessionWithMediaResponse ToResponseWithMedia(this ConferenceSession model)
        {
            TimeOnly? startTime = null;
            TimeOnly? endTime = null;
            DateOnly? date = null;

            if (model.StartTime.HasValue)
            {
                startTime = TimeOnly.FromDateTime(model.StartTime.Value);
                date = DateOnly.FromDateTime(model.StartTime.Value);
            }

            if (model.EndTime.HasValue)
            {
                endTime = TimeOnly.FromDateTime(model.EndTime.Value);
            }

            return new ConferenceSessionWithMediaResponse
            {
                ConferenceSessionId = model.ConferenceSessionId,
                Title = model.Title,
                Description = model.Description,
                StartTime = startTime,
                EndTime = endTime,
                Date = date,
                ConferenceId = model.ConferenceId,
                RoomId = model.RoomId,
                Speakers = model.Speakers?.Select(s => s.ToResponse()).ToList(),
                SessionMedia = model.ConferenceSessionMedia?.Select(m => m.ToResponse()).ToList()
            };
        }

        // Speaker Mappers
        public static Speaker ToModel(this CreateSpeakerRequest request, string conferenceSessionId)
        {
            return new Speaker
            {
                SpeakerId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Image = request.ImageUrl,
                ConferenceSessionId = conferenceSessionId
            };
        }

        public static Speaker ToModel(this UpdateSpeakerRequest request, string conferenceSessionId)
        {
            return new Speaker
            {
                SpeakerId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                ConferenceSessionId = conferenceSessionId
            };
        }

        public static SpeakerResponse ToResponse(this Speaker model)
        {
            return new SpeakerResponse
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.Image
            };
        }

        // Conference Policy Mappers
        public static Policy ToModel(this CreateConferencePolicyRequest request, string conferenceId)
        {
            return new Policy
            {
                PolicyId = Guid.NewGuid().ToString(),
                PolicyName = request.PolicyName,
                Description = request.Description,
                ConferenceId = conferenceId
            };
        }

        public static ConferencePolicyResponse ToResponse(this Policy model)
        {
            return new ConferencePolicyResponse
            {
                PolicyId = model.PolicyId,
                PolicyName = model.PolicyName,
                Description = model.Description
            };
        }

        // Conference Media Mappers
        public static ConferenceMedium ToModel(this CreateConferenceMediaRequest request, string conferenceId)
        {
            return new ConferenceMedium
            {
                ConferenceMediaId = Guid.NewGuid().ToString(),
                ConferenceMediaUrl = request.MediaUrl,
                ConferenceId = conferenceId
            };
        }

        public static ConferenceMediaResponse ToResponse(this ConferenceMedium model)
        {
            return new ConferenceMediaResponse
            {
                MediaId = model.ConferenceMediaId,
                MediaUrl = model.ConferenceMediaUrl
            };
        }

        // Conference Session Media Mappers
        public static ConferenceSessionMedium ToModel(this CreateConferenceSessionMediaRequest request, string conferenceSessionId)
        {
            return new ConferenceSessionMedium
            {
                ConferenceSessionMediaId = Guid.NewGuid().ToString(),
                ConferenceSessionId = conferenceSessionId,
                MediaUrl = request.MediaUrl
            };
        }

        public static ConferenceSessionMediaResponse ToResponse(this ConferenceSessionMedium model)
        {
            return new ConferenceSessionMediaResponse
            {
                MediaId = model.ConferenceSessionMediaId,
                MediaUrl = model.MediaUrl
            };
        }

        // Sponsor Mappers
        public static Sponsor ToModel(this CreateSponsorRequest request, string conferenceId)
        {
            return new Sponsor
            {
                SponsorId = Guid.NewGuid().ToString(),
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                ConferenceId = conferenceId
            };
        }

        public static SponsorResponse ToResponse(this Sponsor model)
        {
            return new SponsorResponse
            {
                SponsorId = model.SponsorId,
                Name = model.Name,
                ImageUrl = model.ImageUrl
            };
        }
    }
}