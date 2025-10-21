using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RefundRequest
{
    public string RefundRequestId { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? TicketId { get; set; }

    public string? GlobalStatusId { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }
}
