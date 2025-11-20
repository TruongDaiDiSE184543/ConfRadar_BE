using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class AuditLog
{
    public string AuditLogId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? EntityName { get; set; }

    public string? ActionDescription { get; set; }

    public virtual User? User { get; set; }
}
