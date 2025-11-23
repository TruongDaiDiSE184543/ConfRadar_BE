using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Conference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceMapper
    {
        public static ConferenceResponse toConferenceResponse(this Conference model)
        {
            return new ConferenceResponse
            {
                Address = model.Address,
                AvailableSlot = model.AvailableSlot,
                BannerImageUrl = model.BannerImageUrl,
                CityId = model.CityId,
                ConferenceCategoryId = model.ConferenceCategoryId,
                ConferenceName = model.ConferenceName,
                ConferenceId = model.ConferenceId,
                ConferenceStatusId = model.ConferenceStatusId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Description = model.Description,
                EndDate = model.EndDate,
                IsInternalHosted = model.IsInternalHosted,
                IsResearchConference = model.IsResearchConference,
                StartDate = model.StartDate,
                TicketSaleEnd = model.TicketSaleEnd,
                TicketSaleStart = model.TicketSaleStart,
                TotalSlot = model.TotalSlot,
            };
        }

        public static ResearchConferencePhaseResponse toResearchPhaseResponse(this ResearchConferencePhase model)
        {
            return new ResearchConferencePhaseResponse
            {
                RegistrationStartDate = model.RegistrationStartDate,
                RegistrationEndDate = model.RegistrationEndDate,
                AbstractDecideStatusStart = model.AbstractDecideStatusStart,
                AbstractDecideStatusEnd = model.AbstractDecideStatusEnd,

                FullPaperStartDate = model.FullPaperStartDate,
                FullPaperEndDate = model.FullPaperEndDate,
                ReviewStartDate = model.ReviewStartDate,
                ReviewEndDate = model.ReviewEndDate,
                FullPaperDecideStatusStart = model.FullPaperDecideStatusStart,
                FullPaperDecideStatusEnd = model.FullPaperDecideStatusEnd,

                ReviseStartDate = model.ReviseStartDate,
                ReviseEndDate = model.ReviseEndDate,
                RevisionPaperReviewStart = model.RevisionPaperReviewStart,
                RevisionPaperReviewEnd = model.RevisionPaperReviewEnd,
                RevisionPaperDecideStatusStart = model.RevisionPaperDecideStatusStart,
                RevisionPaperDecideStatusEnd = model.RevisionPaperDecideStatusEnd,

                CameraReadyStartDate = model.CameraReadyStartDate,
                CameraReadyEndDate = model.CameraReadyEndDate,
                CameraReadyDecideStatusStart = model.CameraReadyDecideStatusStart,
                CameraReadyDecideStatusEnd = model.CameraReadyDecideStatusEnd,

                ConferenceId = model.ConferenceId,
                IsActive = model.IsActive,
                IsWaitlist = model.IsWaitlist,
                ResearchConferencePhaseId = model.ResearchConferencePhaseId,
                RevisionRoundDeadlines = model.RevisionRoundDeadlines.Select(r => r.toRevisionRoundDeadlineResponse()).ToList(),
            };
        }

        public static RevisionRoundDeadlineResponse toRevisionRoundDeadlineResponse(this RevisionRoundDeadline model)
        {
            return new RevisionRoundDeadlineResponse
            {
                ResearchConferencePhaseId = model.ResearchConferencePhaseId,
                RevisionRoundDeadlineId = model.RevisionRoundDeadlineId,
                RoundNumber = model.RoundNumber,
                StartSubmissionDate = model.StartSubmissionDate,
                EndSubmissionDate = model.EndSubmissionDate,
            };
        }
    }
}
