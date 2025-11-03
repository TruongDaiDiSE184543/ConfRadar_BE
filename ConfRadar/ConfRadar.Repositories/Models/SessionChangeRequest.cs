namespace ConfRadar.Repositories.Models;

public partial class SessionChangeRequest
{
    public string SessionChangeRequestId { get; set; } = null!;

    public string? TicketId { get; set; }

    public string? CustomerId { get; set; }

    public string? NewConferenceSessionId { get; set; }

    public string? GlobalStatusId { get; set; }

    public string? Reason { get; set; }

    public DateTime? RequestAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual User? Customer { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual ConferenceSession? NewConferenceSession { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
