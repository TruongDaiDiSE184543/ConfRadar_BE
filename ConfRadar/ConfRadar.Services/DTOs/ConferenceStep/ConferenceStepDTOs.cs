using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.ConferenceStep
{
    // Step 1: Basic Conference Information
    public class CreateConferenceBasicRequest
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

        public IFormFile? BannerImageFile { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(50)]
        public string CategoryName { get; set; }

        public string? LocationId { get; set; }
        public string? GlobalStatusId { get; set; }
    }

    // Step 2: Price Phase and Conference Prices
    public class CreatePricePhaseRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        public DateOnly? EarlierBirdEndInterval { get; set; }

        [Range(0, 100, ErrorMessage = "PercentForEarly must be between 0 and 100")]
        public int? PercentForEarly { get; set; }

        public DateOnly? StandardEndInterval { get; set; }

        public DateOnly? LateEndInterval { get; set; }

        [Range(0, 200, ErrorMessage = "PercentForEnd must be between 0 and 200")]
        public int? PercentForEnd { get; set; }
    }

    public class CreateConferencePriceRequest
    {
        [Required]
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        [Required]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        [Required]
        public string? TicketDescription { get; set; }
        [Required]
        public decimal? ActualPrice { get; set; }

        //public string? PricePhaseId { get; set; }
    }

    public class AddConferencePricesRequest
    {
        public CreatePricePhaseRequest? PricePhase { get; set; }
        public List<CreateConferencePriceRequest>? Prices { get; set; }
    }

    // Step 3: Conference Sessions
    public class CreateConferenceSessionRequest
    {
        [Required(ErrorMessage = "Session title is required")]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(500)]
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Required]
        public DateTime? EndTime { get; set; }


        public string? StatusId { get; set; }

        [Required(ErrorMessage = "Room ID is required for the session")]

        public string? RoomId { get; set; }
        [Required(ErrorMessage = "At least one speaker is needed")]
        public CreateSpeakerRequest? Speaker { get; set; }
    }

    public class CreateSpeakerRequest
    {
        [Required(ErrorMessage = "Speaker name is required")]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class AddConferenceSessionsRequest
    {
        public List<CreateConferenceSessionRequest>? Sessions { get; set; }
    }

    // Step 4: Conference Policies
    public class CreateConferencePolicyRequest
    {
        [MaxLength(255)]
        [Required]
        public string? PolicyName { get; set; }
        [Required]
        public string? Description { get; set; }
    }

    public class AddConferencePoliciesRequest
    {
        public List<CreateConferencePolicyRequest>? Policies { get; set; }
    }

    // Add missing ConferencePolicyResponse
    public class ConferencePolicyResponse
    {
        public string PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string? Description { get; set; }
    }

    // Add missing ConferenceMediaResponse
    public class ConferenceMediaResponse
    {
        public string MediaId { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaTypeId { get; set; }
    }

    // Add missing SponsorResponse
    public class SponsorResponse
    {
        public string SponsorId { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }

    // Step 5: Conference Media
    public class CreateConferenceMediaRequest
    {
        public IFormFile? MediaFile { get; set; }

        [Url(ErrorMessage = "Media URL must be a valid URL")]
        public string? MediaUrl { get; set; }

        public string? MediaTypeId { get; set; }
    }

    public class AddConferenceMediaRequest
    {
        public List<CreateConferenceMediaRequest>? Media { get; set; }
    }

    // Step 6: Conference Sponsors
    public class CreateSponsorRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }
    }

    public class AddConferenceSponsorsRequest
    {
        public List<CreateSponsorRequest>? Sponsors { get; set; }
    }

    // Update Requests for individual components
    public class UpdateConferenceBasicRequest
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

        public IFormFile? BannerImageFile { get; set; }

        public bool? IsInternalHosted { get; set; }

        public bool? IsResearchConference { get; set; }

        public bool? IsActive { get; set; }

        public string? LocationId { get; set; }
        public string? GlobalStatusId { get; set; }
    }

    public class UpdateConferencePriceRequest
    {
        public decimal? TicketPrice { get; set; }

        [MaxLength(255)]
        public string? TicketName { get; set; }

        [MaxLength(500)]
        public string? TicketDescription { get; set; }

        public decimal? ActualPrice { get; set; }

        public string? PricePhaseId { get; set; }
    }

    public class UpdateConferenceSessionRequest
    {
        [MaxLength(50)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }


        public string? StatusId { get; set; }

        public string? RoomId { get; set; }
    }

    public class UpdateSpeakerRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateConferencePolicyRequest
    {
        [MaxLength(255)]
        public string? PolicyName { get; set; }

        public string? Description { get; set; }
    }

    public class UpdateConferenceMediaRequest
    {
        public IFormFile? MediaFile { get; set; }

        [Url(ErrorMessage = "Media URL must be a valid URL")]
        public string? MediaUrl { get; set; }

        public string? MediaTypeId { get; set; }
    }

    public class UpdateSponsorRequest
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }
    }

    // Response DTOs
    public class ConferenceStepResponse
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
    }

    public class ConferencePriceStepResponse
    {
        public string PriceId { get; set; }
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? CurrentPhase { get; set; }
        public string? PricePhaseId { get; set; }
    }

    public class ConferenceSessionStepResponse
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ConferenceId { get; set; }
        public string? RoomId { get; set; }
        public RoomInfoResponse? Room { get; set; }
        public SpeakerResponse? Speaker { get; set; }
    }

    public class RoomInfoResponse
    {
        public string RoomId { get; set; }
        public string? Number { get; set; }
        public string? DisplayName { get; set; }
        public string? DestinationId { get; set; }
    }

    public class SpeakerResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}