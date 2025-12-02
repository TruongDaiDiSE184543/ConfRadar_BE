using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserSuspendHistory
{
    public string SuspendId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime? SuspendedAt { get; set; }

    public DateTime? ResumedAt { get; set; }

    public bool? IsActiveSuspend { get; set; }

    public virtual User User { get; set; } = null!;
}
