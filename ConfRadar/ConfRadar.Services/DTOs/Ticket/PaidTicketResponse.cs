namespace ConfRadar.Services.DTOs.Ticket
{
    public class PaidTicketResponse
    {
        public string? TicketId { get; set; } 
        public string? UserId { get; set; } 
        public string? UserName { get; set; } 
        public bool? IsRefunded { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; } 
        public DateOnly? RegisteredDate { get; set; }
        public string? ConferenceId { get; set; } 
        public string? ConferenceName { get; set; }

    }
}
