using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Conference;
using System.Data;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceMapper
    {
        public static ConferenceResponseDTO toConferenceResponse(this Conference model)
        {
            return new ConferenceResponseDTO
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
                //RevisionPaperReviewStart = model.RevisionPaperReviewStart,
                //RevisionPaperReviewEnd = model.RevisionPaperReviewEnd,
                RevisionPaperDecideStatusStart = model.RevisionPaperDecideStatusStart,
                RevisionPaperDecideStatusEnd = model.RevisionPaperDecideStatusEnd,

                CameraReadyStartDate = model.CameraReadyStartDate,
                CameraReadyEndDate = model.CameraReadyEndDate,
                //CameraReadyDecideStatusStart = model.CameraReadyDecideStatusStart,
                //CameraReadyDecideStatusEnd = model.CameraReadyDecideStatusEnd,
                AuthorPaymentStart = model.AuthorPaymentStart,
                AuthorPaymentEnd = model.AuthorPaymentEnd,

                ConferenceId = model.ConferenceId,
                IsActive = model.IsActive,
                PhaseOrder = model.PhaseOrder,
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

        public static ConferencePolicyResponse ToConferencePolicyResponse(this Policy model)
        {
            return new ConferencePolicyResponse
            {
                Description = model.Description,
                PolicyId = model.PolicyId,
                PolicyName = model.PolicyName
            };
        }

        public static RankingFileUrlResponse ToRankingFileUrlResponse(this RankingFileUrl model)
        {
            return new RankingFileUrlResponse
            {
                RankingFileUrlId = model.RankingFileUrlId,
                FileUrl = model.FileUrl,
            };
        }

        public static MaterialDownloadResponse ToMaterialDownloadResponse(this MaterialDownload model)
        {
            return new MaterialDownloadResponse
            {
                MaterialDownloadId = model.MaterialDownloadId,
                FileDescription = model.FileDescription,
                FileUrl = model.FileName,
            };
        }

        public static RankingReferenceUrlResponse ToRankingReferenceUrlResponse(this RankingReferenceUrl model)
        {
            return new RankingReferenceUrlResponse
            {
                ReferenceUrlId = model.ReferenceUrlId,
                ReferenceUrl = model.ReferenceUrl
            };
        }

        public static SponsorResponse ToSponsorResponse(this Sponsor model)
        {
            return new SponsorResponse
            {
                SponsorId = model.SponsorId,
                Name = model.Name,
                ImageUrl = model.ImageUrl,
            };
        }

        public static RoomInfoResponse ToRoomInfoResponse(this Room model)
        {
            if (model == null)
                return null;
            return new RoomInfoResponse
            {
                RoomId = model?.RoomId,
                DisplayName = model?.DisplayName,
                CityId = model?.Destination?.CityId,
                Cityname = model?.Destination?.City?.CityName,
                DestinationId = model?.Destination?.DestinationId,
                DestinationName = model?.Destination?.Name,
                Number = model?.Number,
            };
        }

        public static ConferenceSessionMediaResponse ToConferenceSessionMediaResponse(this ConferenceSessionMedium model)
        {
            return new ConferenceSessionMediaResponse
            {
                ConferenceSessionMediaId = model.ConferenceSessionMediaId,
                ConferenceSessionMediaUrl = model.MediaUrl
            };
        }

        public static ConferenceMediaResponse ToConferenceMediaResponse(this ConferenceMedium model)
        {
            return new ConferenceMediaResponse
            {
                MediaId = model.ConferenceMediaId,
                MediaUrl = model.ConferenceMediaUrl
            };
        }

        public static RefundPolicyResponse ToRefundPolicyResponse(this RefundPolicy model)
        {
            return new RefundPolicyResponse
            {
                RefundPolicyId = model.RefundPolicyId,
                RefundOrder = model.RefundOrder,
                PercentRefund = model.PercentRefund,
                RefundDeadline = model.RefundDeadline,
                PricePhaseID = model.PricePhaseId,
            };
        }

        public static PricePhaseResponse ToPricePhaseResponse(this PricePhase model)
        {
            return new PricePhaseResponse
            {
                PricePhaseId = model.PricePhaseId,
                PhaseName = model.PhaseName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ApplyPercent = model.ApplyPercent,
                TotalSlot = model.TotalSlot,
                AvailableSlot = model.AvailableSlot,
                RefundPolicies = model?.RefundPolicies.Select(rp => rp.ToRefundPolicyResponse()).OrderBy(rp => rp.RefundOrder).ToList(),
            };
        }

        public static ResearchSessionWithMediaResponse ToResearchSessionWithMediaResponse(this ConferenceSession model)
        {
            return new ResearchSessionWithMediaResponse
            {
                ConferenceSessionId = model.ConferenceSessionId,
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime.HasValue ? TimeOnly.FromDateTime(model.StartTime.Value) : null,
                EndTime = model.EndTime.HasValue ? TimeOnly.FromDateTime(model.EndTime.Value) : null,
                Date = model.SessionDate,
                ConferenceId = model.ConferenceId,
                RoomId = model.RoomId,
                Room = model.Room != null ? model.Room.ToRoomInfoResponse() : null,
                SessionMedia = model.ConferenceSessionMedia?.Select(csm => csm.ToConferenceSessionMediaResponse()).ToList(),
                feedbacks = model.ConferenceFeedbacks?.Select(f => f.ToConferenceSessionFeedbackResponse()).ToList()
            };
        }

        public static ConferencePriceWithPhasesResponse ToConferencePriceWithPhasesResponse(this ConferencePrice model)
        {
            return new ConferencePriceWithPhasesResponse
            {
                ConferencePriceId = model.ConferencePriceId,
                TicketPrice = model.TicketPrice,
                TicketName = model.TicketName,
                TicketDescription = model.TicketDescription,
                IsAuthor = model.IsAuthor,
                IsPublish = model.IsPublish,
                TotalSlot = model.TotalSlot,
                AvailableSlot = model.AvailableSlot,
                PricePhases = model.PricePhases?.Select(pp => pp.ToPricePhaseResponse()).ToList()
            };
        }

        public static SpeakerResponse ToSpeakerResponse(this Speaker model)
        {
            return new SpeakerResponse
            {
                SpeakerId = model.SpeakerId,
                Name = model.Name,
                Description = model.Description,
                Image = model.Image
            };
        }

        public static ConferenceSessionWithSpeakersResponse ToConferenceSessionWithSpeakersResponse(this ConferenceSession model)
        {
            return new ConferenceSessionWithSpeakersResponse
            {
                ConferenceSessionId = model.ConferenceSessionId,
                Title = model.Title,
                Description = model.Description,
                StartTime =  model.StartTime.HasValue ? TimeOnly.FromDateTime(model.StartTime.Value) : null,//model.StartTime,
                EndTime =  model.EndTime.HasValue ? TimeOnly.FromDateTime(model.EndTime.Value) : null,//model.EndTime,
                SessionDate = model.SessionDate,
                ConferenceId = model.ConferenceId,
                RoomId = model.RoomId,
                Speakers = model.Speakers?.Select(s => s.ToSpeakerResponse()).ToList(),
                SessionMedia = model.ConferenceSessionMedia?.Select(csm => csm.ToConferenceSessionMediaResponse()).ToList(),
                Room = model.Room != null ? model?.Room?.ToRoomInfoResponse() : null,
                feedback = model.ConferenceFeedbacks?.Select(f => f.ToConferenceSessionFeedbackResponse()).ToList()

            };
        }

        public static ConferenceTimelineResponse ToConferenceTimelineResponse(this ConferenceTimeline model)
        {
            return new ConferenceTimelineResponse
            {
                ConferenceTimelineId = model.ConferenceTimelineId,
                ConferenceId = model.ConferenceId,
                ChangeDate = model.ChangeDate,
                PreviousStatusId = model.PreviousStatusId,
                AfterwardStatusId = model.AfterwardStatusId,
                Reason = model.Reason,
                PreviousStatusName = model.PreviousStatus?.ConferenceStatusName,
                AfterwardStatusName = model.AfterwardStatus?.ConferenceStatusName,
                ConferenceName = model.Conference?.ConferenceName
            };
        }

        public static CollaboratorContractResponseForConferenceDetail toCollaboratorContractResponseForConferenceDetail(this CollaboratorContract model)
        {
            return new CollaboratorContractResponseForConferenceDetail
            {
                CollaboratorContractId = model.CollaboratorContractId,
                Commission = model.Commission,
                ContractUrl = model.ContractUrl,
                FinalizePaymentDate = model.FinalizePaymentDate,
                IsClosed = model.IsClosed,
                IsMediaStep = model.IsMediaStep,
                IsPolicyStep = model.IsPolicyStep,
                IsPriceStep = model.IsPriceStep,
                IsSessionStep = model.IsSessionStep,
                IsSponsorStep = model.IsSponsorStep,
                IsTicketSelling = model.IsTicketSelling,
                SignDay = model.SignDay
            };
        }

        public static ResearchDetailForWithPriceEndpoint ToResearchDetailForWithPriceEndpoint(this ResearchConferenceDetail model)
        {
            return new ResearchDetailForWithPriceEndpoint
            {
                AllowListener = model.AllowListener,
                NumberPaperAccept = model.NumberPaperAccept,
                PaperFormat = model.PaperFormat,
                RankingCategoryId = model.RankingCategoryId,
                RankingCategoryName = model.RankingCategory.RankName,
                RankingDescription = model.RankingDescription,
                RankValue = model.RankValue,
                RankYear = model.RankYear,
                SubmitPaperFee = model.SubmitPaperFee,
                RevisionAttemptAllowed = model.RevisionAttemptAllowed
            };
        }

        public static ConferenceSessionFeedbackResponse ToConferenceSessionFeedbackResponse(this ConferenceFeedback model)
        {
            return new ConferenceSessionFeedbackResponse
            {
                createdAt = model.CreatedAt,
                Message = model.Message,
                rating = model.Rating,
                UserEmail = model.User.Email,
                UserName = model.User.FullName
            };
        }
    }
}
