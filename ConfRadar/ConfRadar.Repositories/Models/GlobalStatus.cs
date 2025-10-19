using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class GlobalStatus
{
    public string GlobalStatusId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}
