using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Report
{
    public string ReportId { get; set; } = null!;

    public string? ReportSubject { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    public bool? HasResolve { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UserId { get; set; }

    public virtual ReportFeedback? ReportFeedback { get; set; }

    public virtual User? User { get; set; }
}
