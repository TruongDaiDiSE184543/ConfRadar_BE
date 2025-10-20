using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Conference
{
    public class CreateConferenceRequest
    {
        [Required(ErrorMessage = "Conference name is required")]
        [MaxLength(255)]
        public string ConferenceName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateOnly? EndDate { get; set; }

        public int? Capacity { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Url(ErrorMessage = "Banner image URL must be a valid URL")]
        public string? BannerImageUrl { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(50)]
        public string CategoryName { get; set; }

        public string? GlobalStatusId { get; set; }

        public List<ConferencePolicyRequest>? Policies { get; set; }
        public List<ConferenceMediaRequest>? Media { get; set; }
        public List<SponsorRequest>? Sponsors { get; set; }
        public List<ConferencePriceRequest>? Prices { get; set; }
        public List<ConferenceSessionRequest>? Sessions { get; set; }
        public DestinationRequest? Destination { get; set; }
    }

    public class ConferencePolicyRequest
    {
        [MaxLength(255)]
        public string? PolicyName { get; set; }

        public string? Description { get; set; }
    }

    public class ConferenceMediaRequest
    {
        [Url(ErrorMessage = "Media URL must be a valid URL")]
        public string? MediaUrl { get; set; }

        public string? MediaTypeId { get; set; }
    }

    public class SponsorRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }
    }

    public class ConferencePriceRequest
    {
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        public string? TicketDescription { get; set; }

        public decimal? ActualPrice { get; set; }

        [Required(ErrorMessage = "Price phase ID is required")]
        public string? PricePhaseId { get; set; }
    }

    public class ConferenceSessionRequest
    {
        [Required(ErrorMessage = "Session title is required")]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? Date { get; set; }

        public string? StatusId { get; set; }

        public string? RoomId { get; set; }

        public SpeakerRequest? Speaker { get; set; }
    }

    public class SpeakerRequest
    {
        [Required(ErrorMessage = "Speaker name is required")]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class DestinationRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? District { get; set; }

        [MaxLength(50)]
        public string? Street { get; set; }
    }
}