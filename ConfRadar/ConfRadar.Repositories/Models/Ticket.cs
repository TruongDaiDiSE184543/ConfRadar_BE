namespace ConfRadar.Repositories.Models;

public partial class Ticket
{
    public string TicketId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? ConferencePriceId { get; set; }

    public DateTime? RegisteredDate { get; set; }

    public bool? IsRefunded { get; set; }

    public decimal? ActualPrice { get; set; }

    public virtual ConferencePrice? ConferencePrice { get; set; }

    public virtual RefundRequest? RefundRequest { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
