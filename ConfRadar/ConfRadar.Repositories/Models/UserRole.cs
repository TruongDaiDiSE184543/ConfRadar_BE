using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserRole
{
    public string Userid { get; set; } = null!;

    public string Roleid { get; set; } = null!;

    public DateTime? Assignedat { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
