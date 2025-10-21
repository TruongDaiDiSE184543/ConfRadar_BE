using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Transaction
{
    public string TransactionId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Currency { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? TransactionStatusId { get; set; }

    public string? TransactionTypeId { get; set; }

    public string? PaymentMethodId { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual TransactionStatus? TransactionStatus { get; set; }

    public virtual TransactionType? TransactionType { get; set; }

    public virtual User? User { get; set; }
}
