using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.ConferencePriceTicket
{
    public class ConferencePriceTicketListResponse
    {
        public string PriceId { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public decimal? TicketPrice { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? CurrentPhase { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceBannerUrl { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }
        public DateOnly? ConferenceEndDate { get; set; }
    }

    public class ConferencePriceTicketDetailResponse
    {
        public string PriceId { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public decimal? TicketPrice { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? CurrentPhase { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public string? ConferenceBannerUrl { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }
        public DateOnly? ConferenceEndDate { get; set; }
        public PricePhaseInfoResponse? PricePhase { get; set; }
    }

    public class PricePhaseInfoResponse
    {
        public string PricePhaseId { get; set; }
        public string? Name { get; set; }
        public DateOnly? EarlierBirdEndInterval { get; set; }
        public int? PercentForEarly { get; set; }
        public DateOnly? StandardEndInterval { get; set; }
        public DateOnly? LateEndInterval { get; set; }
        public int? PercentForEnd { get; set; }
    }

    public class ConferencePriceTicketSearchRequest
    {
        [MaxLength(100)]
        public string? TicketName { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "TicketPrice"; // Default sort by price

        public string? SortOrder { get; set; } = "asc"; // Default sort ascending

        public int Page { get; set; } = 1; // Default page 1

        public int PageSize { get; set; } = 10; // Default 10 items per page
    }
}