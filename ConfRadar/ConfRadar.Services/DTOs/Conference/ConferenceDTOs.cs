using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Conference
{
    public class ConferenceResponse
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
        public DateOnly? CreatedAt { get; set; }
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
        public DateOnly? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
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
        public DateOnly? CreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public string? CityId { get; set; }
        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }
        public string? TargetAudience { get; set; } // Technical conference detail
        public List<ConferencePolicyResponse>? Policies { get; set; }
        public List<SponsorResponse>? Sponsors { get; set; }
        public List<ConferenceSessionWithSpeakersResponse>? Sessions { get; set; }
        public List<ConferencePriceWithPhasesResponse>? ConferencePrices { get; set; }
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
}