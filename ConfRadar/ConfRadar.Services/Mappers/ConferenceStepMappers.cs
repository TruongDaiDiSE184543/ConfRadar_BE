using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.ConferenceStep;

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
                ConferenceId = conferenceId,

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
        public static PricePhase ToModel(this CreatePricePhaseRequest request, string conferencePriceId, string researchPhaseId)
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
                ConferencePriceId = conferencePriceId,
                ResearchConferencePhaseId = researchPhaseId
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
                AvailableSlot = model.AvailableSlot,
                ConferencePriceId = model.ConferencePriceId,
                ResearchConferencePhaseId = model.ResearchConferencePhaseId
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
        public static Speaker ToModel(this CreateSpeakerRequest request, string conferenceSessionId, string imageURL)
        {
            return new Speaker
            {
                SpeakerId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Image = imageURL,
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
                SpeakerId = model.SpeakerId,
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.Image,
                ConferenceSessionId = model.ConferenceSessionId
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
        public static ConferenceSessionMedium ToModel(this CreateConferenceSessionMediaRequest request, string conferenceSessionId, string mediaURL)
        {
            return new ConferenceSessionMedium
            {
                ConferenceSessionMediaId = Guid.NewGuid().ToString(),
                ConferenceSessionId = conferenceSessionId,
                MediaUrl = mediaURL
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

        // Refund Policy Mappers
        public static RefundPolicy ToModel(this CreateRefundPolicyRequest request, string conferenceId)
        {
            return new RefundPolicy
            {
                RefundPolicyId = Guid.NewGuid().ToString(),
                PercentRefund = request.PercentRefund,
                RefundDeadline = request.RefundDeadline,
                //RefundOrder = request.RefundOrder,
                ConferenceId = conferenceId
            };
        }

        public static RefundPolicyResponse ToResponse(this RefundPolicy model)
        {
            return new RefundPolicyResponse
            {
                RefundPolicyId = model.RefundPolicyId,
                PercentRefund = model.PercentRefund,
                RefundDeadline = model.RefundDeadline,
                RefundOrder = model.RefundOrder,
                pricePhaseId = model.PricePhaseId
            };
        }

        // Research Conference Mappers
        public static Conference ToModel(this CreateResearchConferenceBasicRequest request, ConferenceStatus status, DateTime createdAt)
        {
            return new Conference
            {
                ConferenceId = Guid.NewGuid().ToString(),
                ConferenceName = request.ConferenceName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSlot = request.TotalSlot,
                AvailableSlot = request.TotalSlot,
                Address = request.Address,
                BannerImageUrl = request.bannerImageFileUrl,
                CreatedAt = createdAt,
                CreatedBy = request.createdby,
                IsInternalHosted = request.IsInternalHosted ?? true,
                IsResearchConference = request.IsResearchConference ?? true,
                ConferenceCategoryId = request.ConferenceCategoryId,
                CityId = request.CityId,
                TicketSaleStart = request.TicketSaleStart,
                TicketSaleEnd = request.TicketSaleEnd,
                ConferenceStatusId = status.ConferenceStatusId
            };
        }

        public static ResearchConferenceBasicStepResponse ToResearchResponse(this Conference model)
        {
            return new ResearchConferenceBasicStepResponse
            {
                conferenceId = model.ConferenceId,
                ConferenceName = model.ConferenceName,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalSlot = model.TotalSlot,
                AvailableSlot = model.AvailableSlot,
                Address = model.Address,
                bannerImageFileUrl = model.BannerImageUrl,
                createdAt = model.CreatedAt,
                IsInternalHosted = model.IsInternalHosted,
                IsResearchConference = model.IsResearchConference,
                createdby = model.CreatedBy,
                CityId = model.CityId,
                ConferenceCategoryId = model.ConferenceCategoryId,
                TicketSaleStart = model.TicketSaleStart,
                TicketSaleEnd = model.TicketSaleEnd
                // Note: No target audience for research conference response
            };
        }

        // Research Session Mappers
        public static ConferenceSession ToModel(this CreateResearchSessionRequest request, string conferenceId)
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

        public static ResearchSessionWithMediaResponse ToResearchResponseWithMedia(this ConferenceSession model)
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

            return new ResearchSessionWithMediaResponse
            {
                ConferenceSessionId = model.ConferenceSessionId,
                Title = model.Title,
                Description = model.Description,
                StartTime = startTime,
                EndTime = endTime,
                Date = date,
                ConferenceId = model.ConferenceId,
                RoomId = model.RoomId,
                SessionMedia = model.ConferenceSessionMedia?.Select(m => m.ToResponse()).ToList()
            };
        }

        // Research Conference Detail Mappers
        public static ResearchConferenceDetail ToModel(this CreateResearchConferenceDetailRequest request, string conferenceId)
        {
            return new ResearchConferenceDetail
            {
                ConferenceId = conferenceId,
                Name = request.Name,
                PaperFormat = request.PaperFormat,
                NumberPaperAccept = request.NumberPaperAccept,
                RevisionAttemptAllowed = request.RevisionAttemptAllowed,
                RankingDescription = request.RankingDescription,
                AllowListener = request.AllowListener,
                RankValue = request.RankValue,
                RankYear = request.RankYear,
                ReviewFee = request.ReviewFee,
                RankingCategoryId = request.RankingCategoryId
            };
        }

        public static ResearchConferenceDetailResponse ToResponse(this ResearchConferenceDetail model)
        {
            return new ResearchConferenceDetailResponse
            {
                ConferenceId = model.ConferenceId,
                Name = model.Name,
                PaperFormat = model.PaperFormat,
                NumberPaperAccept = model.NumberPaperAccept,
                RevisionAttemptAllowed = model.RevisionAttemptAllowed,
                RankingDescription = model.RankingDescription,
                AllowListener = model.AllowListener,
                RankValue = model.RankValue,
                RankYear = model.RankYear,
                ReviewFee = model.ReviewFee,
                RankingCategoryId = model.RankingCategoryId,
                RankingCategoryName = model.RankingCategory?.RankName // Include related RankingCategory name
            };
        }

        // Research Conference Phase Mappers
        public static ResearchConferencePhase ToModel(this CreateResearchConferencePhaseItemRequest request, string conferenceId)
        {
            return new ResearchConferencePhase
            {
                ResearchConferencePhaseId = Guid.NewGuid().ToString(),
                ConferenceId = conferenceId,
                RegistrationStartDate = request.RegistrationStartDate,
                RegistrationEndDate = request.RegistrationEndDate,
                FullPaperStartDate = request.FullPaperStartDate,
                FullPaperEndDate = request.FullPaperEndDate,
                ReviewStartDate = request.ReviewStartDate,
                ReviewEndDate = request.ReviewEndDate,
                ReviseStartDate = request.ReviseStartDate,
                ReviseEndDate = request.ReviseEndDate,
                CameraReadyStartDate = request.CameraReadyStartDate,
                CameraReadyEndDate = request.CameraReadyEndDate,
                IsWaitlist = request.IsWaitlist
            };
        }

        public static ResearchConferencePhaseResponse ToResponse(this ResearchConferencePhase model)
        {
            return new ResearchConferencePhaseResponse
            {
                ResearchConferencePhaseId = model.ResearchConferencePhaseId,
                ConferenceId = model.ConferenceId,
                RegistrationStartDate = model.RegistrationStartDate,
                RegistrationEndDate = model.RegistrationEndDate,
                FullPaperStartDate = model.FullPaperStartDate,
                FullPaperEndDate = model.FullPaperEndDate,
                ReviewStartDate = model.ReviewStartDate,
                ReviewEndDate = model.ReviewEndDate,
                ReviseStartDate = model.ReviseStartDate,
                ReviseEndDate = model.ReviseEndDate,
                CameraReadyStartDate = model.CameraReadyStartDate,
                CameraReadyEndDate = model.CameraReadyEndDate,
                IsWaitlist = model.IsWaitlist,
                IsActive = model.IsActive,
                RevisionRoundDeadlines = model.RevisionRoundDeadlines?.Select(r => r.ToResponse()).ToList()
            };
        }

        // Revision Round Deadline Mappers
        //public static RevisionRoundDeadline ToModel(this CreateRevisionRoundDeadlineRequest request, string researchConferencePhaseId)
        //{
        //    return new RevisionRoundDeadline
        //    {
        //        RevisionRoundDeadlineId = Guid.NewGuid().ToString(),
        //        StartSubmissionDate = request.StartSubmissionDate,
        //        EndSubmissionDate = request.EndSubmissionDate,
        //        RoundNumber = request.RoundNumber,
        //        ResearchConferencePhaseId = researchConferencePhaseId
        //    };
        //}

        //public static RevisionRoundDeadline ToModel(this UpdateRevisionRoundDeadlineRequest request)
        //{
        //    return new RevisionRoundDeadline
        //    {
        //        EndSubmissionDate = request.EndDate,
        //        RoundNumber = request.RoundNumber
        //    };
        //}

        public static RevisionRoundDeadlineResponse ToResponse(this RevisionRoundDeadline model)
        {
            return new RevisionRoundDeadlineResponse
            {
                RevisionRoundDeadlineId = model.RevisionRoundDeadlineId,
                StartSubmissionDate = model.StartSubmissionDate,
                EndSubmissionDate = model.EndSubmissionDate,
                RoundNumber = model.RoundNumber,
                ResearchConferencePhaseId = model.ResearchConferencePhaseId
            };
        }

        public static RevisionRoundDeadlineResponse ToRevisionRoundDeadlineResponse(this RevisionRoundDeadline model)
        {
            return new RevisionRoundDeadlineResponse
            {
                RevisionRoundDeadlineId = model.RevisionRoundDeadlineId,
                StartSubmissionDate = model.StartSubmissionDate,
                EndSubmissionDate = model.EndSubmissionDate,
                RoundNumber = model.RoundNumber,
                ResearchConferencePhaseId = model.ResearchConferencePhaseId
            };
        }

        // Material Download Mappers
        public static MaterialDownload ToModel(this CreateMaterialDownloadRequest request, string conferenceId,string fileName)
        {
            return new MaterialDownload
            {
                MaterialDownloadId = Guid.NewGuid().ToString(),
                FileName = fileName,
                FileDescription = request.FileDescription,
                ConferenceId = conferenceId
            };
        }

        public static MaterialDownloadResponse ToResponse(this MaterialDownload model)
        {
            return new MaterialDownloadResponse
            {
                MaterialDownloadId = model.MaterialDownloadId,
                FileName = model.FileName,
                FileDescription = model.FileDescription,
                FileUrl = model.FileName // For file download, the file URL would be constructed based on the filename
            };
        }

        // Ranking File URL Mappers
        public static RankingFileUrl ToModel(this CreateRankingFileUrlRequest request, string conferenceId)
        {
            return new RankingFileUrl
            {
                RankingFileUrlId = Guid.NewGuid().ToString(),
                FileUrl = request.FileUrl,
                ConferenceId = conferenceId
            };
        }

        public static RankingFileUrlResponse ToResponse(this RankingFileUrl model)
        {
            return new RankingFileUrlResponse
            {
                RankingFileUrlId = model.RankingFileUrlId,
                FileUrl = model.FileUrl
            };
        }

        // Ranking Reference URL Mappers
        public static RankingReferenceUrl ToModel(this CreateRankingReferenceUrlRequest request, string conferenceId)
        {
            return new RankingReferenceUrl
            {
                ReferenceUrlId = Guid.NewGuid().ToString(),
                ReferenceUrl = request.ReferenceUrl,
                ConferenceId = conferenceId
            };
        }

        public static RankingReferenceUrlResponse ToResponse(this RankingReferenceUrl model)
        {
            return new RankingReferenceUrlResponse
            {
                ReferenceUrlId = model.ReferenceUrlId,
                ReferenceUrl = model.ReferenceUrl
            };
        }

        // Price Phase Mappers - Extension methods for PricePhase DTOs
        public static PricePhase ToModel(this CreatePricePhaseRequestForConferencePrice request, string conferencePriceId, string? researchPhaseId)
        {
            return new PricePhase
            {
                PricePhaseId = Guid.NewGuid().ToString(),
                PhaseName = request.PhaseName,
                ApplyPercent = request.ApplyPercent,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSlot = request.TotalSlot,
                AvailableSlot = request.TotalSlot, // Initialize available slot to total slot
                ConferencePriceId = conferencePriceId,
                ResearchConferencePhaseId = researchPhaseId
            };
        }

        public static PricePhase ToModel(this UpdatePricePhaseRequest request)
        {
            return new PricePhase
            {
                PhaseName = request.PhaseName,
                ApplyPercent = request.ApplyPercent,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSlot = request.TotalSlot,
                AvailableSlot = request.TotalSlot // Update available slot to match total slot if total is updated
            };
        }

        // Speaker Mappers - Extension methods for Speaker DTOs
        public static Speaker ToModel(this CreateSpeakerRequestForConferenceSession request, string conferenceSessionId, string? imageURL)
        {
            return new Speaker
            {
                SpeakerId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Image = imageURL,
                ConferenceSessionId = conferenceSessionId
            };
        }

        public static Speaker ToModel(this UpdateSpeakerRequestForConferenceSession request)
        {
            return new Speaker
            {
                Name = request.Name,
                Description = request.Description
            };
        }

    }
}