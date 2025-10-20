using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Conference
{
    public class ConferenceResponse
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? Capacity { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }
        public bool? IsActive { get; set; }
        public string? UserId { get; set; }
        public string? LocationId { get; set; }
        public string? CategoryId { get; set; }

        public List<ConferencePolicyResponse>? Policies { get; set; }
        public List<ConferenceMediaResponse>? Media { get; set; }
        public List<SponsorResponse>? Sponsors { get; set; }
        public List<ConferencePriceResponse>? Prices { get; set; }
        public List<ConferenceSessionResponse>? Sessions { get; set; }
    }

    public class ConferencePolicyResponse
    {
        public string PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string? Description { get; set; }
    }

    public class ConferenceMediaResponse
    {
        public string MediaId { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaTypeId { get; set; }
    }

    public class SponsorResponse
    {
        public string SponsorId { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ConferencePriceResponse
    {
        public string PriceId { get; set; }
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? PricePhaseId { get; set; }
    }

    public class ConferenceSessionResponse
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? Date { get; set; }
        public string? ConferenceId { get; set; }
        public string? StatusId { get; set; }
        public string? RoomId { get; set; }
        public SpeakerResponse? Speaker { get; set; }
    }

    public class SpeakerResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateConferenceRequest
    {
        [MaxLength(255)]
        public string? ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int? Capacity { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Url(ErrorMessage = "Banner image URL must be a valid URL")]
        public string? BannerImageUrl { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        public bool? IsActive { get; set; }
    }
}