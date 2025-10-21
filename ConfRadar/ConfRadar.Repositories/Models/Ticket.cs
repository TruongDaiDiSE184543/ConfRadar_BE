using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Ticket
{
    public string TicketId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? ConferencePriceId { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? RegisteredDate { get; set; }

    public bool? IsRefunded { get; set; }

    public decimal? ActualPrice { get; set; }

    public virtual ConferencePrice? ConferencePrice { get; set; }

    public virtual RefundRequest? RefundRequest { get; set; }

    public virtual Transaction? Transaction { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();
}
