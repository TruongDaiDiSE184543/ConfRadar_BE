using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserRole
{
    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
