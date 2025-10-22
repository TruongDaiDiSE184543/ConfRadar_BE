using Microsoft.AspNetCore.Http;
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
        [Required(ErrorMessage = "Capacity is required")]
        public int? Capacity { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public IFormFile? BannerImageFile { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(50)]
        public string CategoryName { get; set; }

        public string? LocationId { get; set; }  // Reference to existing destination
        public string? GlobalStatusId { get; set; }

        public List<ConferencePolicyRequest>? Policies { get; set; }
        public List<ConferenceMediaRequest>? Media { get; set; }
        public List<SponsorRequest>? Sponsors { get; set; }
        public List<ConferencePriceRequest>? Prices { get; set; }
        public List<ConferenceSessionRequest>? Sessions { get; set; }
        public CreatePricePhaseRequest? PricePhase { get; set; }
    }

    public class CreatePricePhaseRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        public DateOnly? EarlierBirdEndInterval { get; set; }

        [Range(0, 200, ErrorMessage = "PercentForEarly must be between 0 and 200")]
        public int? PercentForEarly { get; set; }

        public DateOnly? StandardEndInterval { get; set; }

        public DateOnly? LateEndInterval { get; set; }

        [Range(0, 200, ErrorMessage = "PercentForEnd must be between 0 and 200")]
        public int? PercentForEnd { get; set; }
        public List<ConferenceSessionRequest>? Sessions { get; set; }
        // Remove Destination property since destinations are managed separately
    }

    public class ConferencePolicyRequest
    {
        [MaxLength(255)]
        public string? PolicyName { get; set; }

        public string? Description { get; set; }
    }

    public class ConferenceMediaRequest
    {
        public IFormFile? MediaFile { get; set; }

        [Url(ErrorMessage = "Media URL must be a valid URL")]
        public string? MediaUrl { get; set; }  // Allow URL as fallback

        public string? MediaTypeId { get; set; }
    }

    public class SponsorRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }  // Allow file upload instead of URL
    }

    public class ConferencePriceRequest
    {
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        public string? TicketDescription { get; set; }

        public decimal? ActualPrice { get; set; }


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

        [Required(ErrorMessage = "Room ID is required for the session")]
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
    // Remove DestinationRequest since destinations are managed separately
}