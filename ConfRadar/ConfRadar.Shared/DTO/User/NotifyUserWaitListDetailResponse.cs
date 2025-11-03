namespace ConfRadar.Shared.DTO.User
{
    public class NotifyUserWaitListDetailResponse
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public List<NotifyConferencePriceDetailResponse> ConferencePriceDetailList { get; set; }

    }
    public class NotifyConferencePriceDetailResponse
    {
        public string ConferencePriceId { get; set; } = null!;
        public decimal? TicketPrice { get; set; }
        public string? TicketName { get; set; }
        public string? TicketDescription { get; set; }
        public int? TotalSlot { get; set; }
        public int? AvailableSlot { get; set; }

    }
}
