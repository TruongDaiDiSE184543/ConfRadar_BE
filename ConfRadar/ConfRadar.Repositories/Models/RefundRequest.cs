using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RefundRequest
{
    public string RefundRequestId { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? TicketId { get; set; }

    public string? GlobalStatusId { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual Transaction? Transaction { get; set; }
}
