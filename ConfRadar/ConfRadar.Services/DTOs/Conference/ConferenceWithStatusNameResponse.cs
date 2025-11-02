using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Conference
{
    public class ConferenceWithStatusNameResponse
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
        public string? ConferenceStatusName { get; set; } // Changed from ConferenceStatusId to ConferenceStatusName
    }
    
    public class ConferenceWithStatusNameListResponse
    {
        public List<ConferenceWithStatusNameResponse>? Conferences { get; set; }
    }
}