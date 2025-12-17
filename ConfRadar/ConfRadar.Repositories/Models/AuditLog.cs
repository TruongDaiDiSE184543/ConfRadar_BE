using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class AuditLog
{
    public string AuditLogId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? CategoryId { get; set; }

    public string? ActionDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AuditLogCategory? Category { get; set; }

    public virtual User? User { get; set; }
}
