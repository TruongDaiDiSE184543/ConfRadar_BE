namespace ConfRadar.Repositories.Models;

public partial class Transaction
{
    public string TransactionId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Currency { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? TransactionCode { get; set; }

    public bool? IsRefunded { get; set; }

    public string? PaymentMethodId { get; set; }

    public string? TicketId { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual User? User { get; set; }
}
