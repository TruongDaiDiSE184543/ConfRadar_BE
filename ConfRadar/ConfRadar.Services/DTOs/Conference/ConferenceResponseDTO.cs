using ConfRadar.Services.Mappers;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Conference
{
    public class ConferenceResponseDTO
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? CreatedBy { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
    }

    public class CreateConferenceRequest
    {
        [Required]
        [MaxLength(255)]
        public string ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public int TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string? BannerImageUrl { get; set; }

        [Required]
        public bool IsInternalHosted { get; set; }

        [Required]
        public bool IsResearchConference { get; set; }

        [Required]
        [MaxLength(50)]
        public string ConferenceCategoryId { get; set; }

        [Required]
        public string CityId { get; set; }

        [Required]
        public DateOnly TicketSaleStart { get; set; }

        [Required]
        public DateOnly TicketSaleEnd { get; set; }
    }

    public class UpdateConferenceRequest
    {
        [MaxLength(255)]
        public string? ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int? TotalSlot { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string? BannerImageUrl { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        [MaxLength(50)]
        public string? ConferenceCategoryId { get; set; }

        public string? CityId { get; set; }

        public DateOnly? TicketSaleStart { get; set; }

        public DateOnly? TicketSaleEnd { get; set; }
    }

    // DTOs for endpoint 1: Conferences with prices
    public class ConferencePriceWithPhasesResponse
    {
        public string ConferencePriceId { get; set; } = string.Empty;
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public bool? IsAuthor { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public List<PricePhaseResponse>? PricePhases { get; set; }
    }

    public class PricePhaseResponse
    {
        public string PricePhaseId { get; set; } = string.Empty;
        public string? PhaseName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? ApplyPercent { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public List<RefundPolicyResponse>? RefundPolicies { get; set; }
    }

    public class ConferenceWithPricesResponse
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? targetAudience {  get; set; }
        public ResearchDetailForWithPriceEndpoint ResearchConferenceDetailResponse { get; set; }
        public List<ConferencePriceWithPhasesResponse>? ConferencePrices { get; set; }
    }

    // DTOs for endpoint 2: Technical conference detail
    public class TechnicalConferenceDetailResponse
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? TargetAudience { get; set; } // Technical conference detail
        public string? createdBy { get; set; }
        public string? UserNameCreator { get; set; }
        public string? Organization { get; set; }
        public CollaboratorContractResponseForConferenceDetail Contract { get; set; }
        public List<ConferenceMediaResponse>? ConferenceMedia { get; set; }
        public List<ConferencePolicyResponse>? Policies { get; set; }
        public List<SponsorResponse>? Sponsors { get; set; }
        public List<ConferenceSessionWithSpeakersResponse>? Sessions { get; set; }
        public List<ConferencePriceWithPhasesResponse>? ConferencePrices { get; set; }
        public List<ConferenceTimelineResponse>? ConferenceTimelines { get; set; } // Include conference timeline data
        public PurchasedInfo? purchasedInfo { get; set; }

    }

    public class CollaboratorContractResponseForConferenceDetail
    {
        public string CollaboratorContractId { get; set; }
        public bool? IsSponsorStep { get; set; }

        public bool? IsMediaStep { get; set; }

        public bool? IsPolicyStep { get; set; }

        public bool? IsSessionStep { get; set; }

        public bool? IsPriceStep { get; set; }

        public bool? IsTicketSelling { get; set; }

        public bool? IsClosed { get; set; }

        public DateOnly? SignDay { get; set; }

        public DateOnly? FinalizePaymentDate { get; set; }

        public int? Commission { get; set; }

        public string? ContractUrl { get; set; }

    }

    public class ResearchDetailForWithPriceEndpoint
    {
        public string? PaperFormat { get; set; }
        public int? NumberPaperAccept { get; set; }
        public int? RevisionAttemptAllowed { get; set; }
        public string? RankingDescription { get; set; }
        public bool? AllowListener { get; set; }
        public string? RankValue { get; set; }
        public int? RankYear { get; set; }
        public decimal? ReviewFee { get; set; }
        public string? RankingCategoryId { get; set; }
        public string? RankingCategoryName { get; set; }
    }
    public class ResearchConferenceDetailResponse
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? createdBy { get; set; }

        // Research Conference Detail specific fields
        public string? UserNameCreator { get; set; }
        public string? PaperFormat { get; set; }
        public int? NumberPaperAccept { get; set; }
        public int? RevisionAttemptAllowed { get; set; }
        public string? RankingDescription { get; set; }
        public bool? AllowListener { get; set; }
        public string? RankValue { get; set; }
        public int? RankYear { get; set; }
        public decimal? ReviewFee { get; set; }
        public string? RankingCategoryId { get; set; }
        public string? RankingCategoryName { get; set; }

        // Research Conference related data
        public List<RankingFileUrlResponse>? RankingFileUrls { get; set; }
        public List<MaterialDownloadResponse>? MaterialDownloads { get; set; }
        public List<RankingReferenceUrlResponse>? RankingReferenceUrls { get; set; }
        public List<ResearchConferencePhaseResponse>? ResearchPhase { get; set; }
        public List<ResearchSessionWithMediaResponse>? ResearchSessions { get; set; }

        // Shared tables data (same as technical conference)
        public List<ConferencePolicyResponse>? Policies { get; set; }
        public List<SponsorResponse>? Sponsors { get; set; }
        public List<ConferenceMediaResponse>? ConferenceMedia { get; set; }
        public List<ConferencePriceWithPhasesResponse>? ConferencePrices { get; set; }
        public List<ConferenceTimelineResponse>? ConferenceTimelines { get; set; } 
        public PurchasedInfo? purchasedInfo { get; set; }
    }

    public class PurchasedInfo
    {
        public string ticketId { get; set; }
        public string conferencePriceId { get; set; }
        public string pricePhaseId { get; set; }
    }

    public class ConferencePolicyResponse
    {
        public string PolicyId { get; set; } = string.Empty;
        public string? PolicyName { get; set; }
        public string? Description { get; set; }
    }

    public class SponsorResponse
    {
        public string SponsorId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class RoomInfoResponse
    {
        public string RoomId { get; set; } = string.Empty;
        public string? Number { get; set; }
        public string? DisplayName { get; set; }
        public string CityId { get; set; }
        public string Cityname { get; set; }
        public string DestinationId { get; set; }
        public string DestinationName { get; set; }
    }

    public class ConferenceSessionWithSpeakersResponse
    {
        public string ConferenceSessionId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateOnly? SessionDate { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        public RoomInfoResponse? Room { get; set; } // Include room information
        public List<SpeakerResponse>? Speakers { get; set; }
        public List<ConferenceSessionMediaResponse>? SessionMedia { get; set; }
    }

    public class SpeakerResponse
    {
        public string SpeakerId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    public class ConferenceSessionMediaResponse
    {
        public string ConferenceSessionMediaId { get; set; } = string.Empty;
        public string? ConferenceSessionMediaUrl { get; set; }
    }

    // DTOs for endpoint 4: Step completion status
    public class ConferenceStepCompletionStatusResponse
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public bool IsResearch { get; set; } // false if technical, true if research
        public bool HavePolicy { get; set; }
        public bool HaveSponsor { get; set; }
        public bool HaveSession { get; set; }
        public bool HaveSessionMedia { get; set; }
        public bool HaveSpeakerInSession { get; set; }
        public bool HaveConferencePrice { get; set; }
        public bool HaveTechnicalConferenceDetail { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
    }

    public class ResearchConferenceStepCompletionStatusResponse
    {
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public bool IsResearch { get; set; } // Always true for research conferences
        public bool HaveResearchConferenceDetail { get; set; }
        public bool HaveMaterialDownload { get; set; }
        public bool HaveRankingFileUrl { get; set; }
        public bool HaveRankingReferenceUrl { get; set; }
        public bool HaveResearchPhase { get; set; }
        public bool HaveResearchSession { get; set; }
        public bool HaveResearchSessionMedia { get; set; }
        public bool HavePolicy { get; set; }
        public bool HaveSponsor { get; set; }
        public bool HaveConferencePrice { get; set; }
        public bool HaveRefundPolicy { get; set; }
        public bool HaveConferenceMedia { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
    }

    public class CheckStepCompletionRequest
    {
        public string? Step { get; set; }
    }

    public class ApproveConferenceRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
        public Boolean? IsApprove { get; set; }
    }

    // Additional DTOs for research conference related functionality
    public class RefundPolicyResponse
    {
        public string? RefundPolicyId { get; set; }
        public int? PercentRefund { get; set; }
        public DateOnly? RefundDeadline { get; set; }
        public int? RefundOrder { get; set; }
        public string? PricePhaseID { get; set; }
    }

    public class ConferenceMediaResponse
    {
        public string MediaId { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
    }

    public class RankingFileUrlResponse
    {
        public string? RankingFileUrlId { get; set; }
        public string? FileUrl { get; set; }
    }

    public class MaterialDownloadResponse
    {
        public string? MaterialDownloadId { get; set; }
        public string? FileDescription { get; set; }
        public string? FileUrl { get; set; }
    }

    public class RankingReferenceUrlResponse
    {
        public string? ReferenceUrlId { get; set; }
        public string? ReferenceUrl { get; set; }
    }

    public class ResearchConferencePhaseResponse
    {
        public string? ResearchConferencePhaseId { get; set; }
        public string? ConferenceId { get; set; }
        public DateOnly? RegistrationStartDate { get; set; }
        public DateOnly? RegistrationEndDate { get; set; }
        // Abstract phase decide status dates (conference organizer only)
        public DateOnly? AbstractDecideStatusStart { get; set; }
        public DateOnly? AbstractDecideStatusEnd { get; set; }
        public DateOnly? FullPaperStartDate { get; set; }
        public DateOnly? FullPaperEndDate { get; set; }
        // Full paper review dates (normal reviewers)
        public DateOnly? ReviewStartDate { get; set; }
        public DateOnly? ReviewEndDate { get; set; }
        // Full paper decide status dates (head reviewer)
        public DateOnly? FullPaperDecideStatusStart { get; set; }
        public DateOnly? FullPaperDecideStatusEnd { get; set; }
        public DateOnly? ReviseStartDate { get; set; }
        public DateOnly? ReviseEndDate { get; set; }
        // Revision paper review dates (normal reviewers)
        //public DateOnly? RevisionPaperReviewStart { get; set; }
        //public DateOnly? RevisionPaperReviewEnd { get; set; }
        // Revision paper decide status dates (head reviewer)
        public DateOnly? RevisionPaperDecideStatusStart { get; set; }
        public DateOnly? RevisionPaperDecideStatusEnd { get; set; }
        public DateOnly? CameraReadyStartDate { get; set; }
        public DateOnly? CameraReadyEndDate { get; set; }
        // Camera ready decide status dates (head reviewer only)
        public DateOnly? CameraReadyDecideStatusStart { get; set; }
        public DateOnly? CameraReadyDecideStatusEnd { get; set; }
        public bool? IsWaitlist { get; set; }
        public bool? IsActive { get; set; }
        public List<RevisionRoundDeadlineResponse>? RevisionRoundDeadlines { get; set; }
    }

    public class RevisionRoundDeadlineResponse
    {
        public string? RevisionRoundDeadlineId { get; set; }
        public DateOnly? StartSubmissionDate { get; set; }
        public DateOnly? EndSubmissionDate { get; set; }
        public int? RoundNumber { get; set; }
        public string? ResearchConferencePhaseId { get; set; }
    }

    public class ResearchSessionWithMediaResponse
    {
        public string ConferenceSessionId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateOnly? Date { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        public RoomInfoResponse? Room { get; set; } // Include room information
        public List<ConferenceSessionMediaResponse>? SessionMedia { get; set; }
    }

    public class SkeletonTechConfResponse
    {
        public string? ConferenceId { get; set; }
        public string? Name { get; set; }
        public DateTime? createdAt { get; set; }
    }


}