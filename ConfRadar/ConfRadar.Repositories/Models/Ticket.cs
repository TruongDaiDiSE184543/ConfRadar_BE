using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Ticket
{
    public string TicketId { get; set; } = null!;

    public DateOnly? RegisteredDate { get; set; }

    public bool? IsRefunded { get; set; }

    public decimal? ActualPrice { get; set; }

    public string? UserId { get; set; }

    public string? ConferencePriceId { get; set; }

    public virtual ConferencePrice? ConferencePrice { get; set; }

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();

    public virtual ICollection<SessionChangeRequest> SessionChangeRequests { get; set; } = new List<SessionChangeRequest>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();
}
