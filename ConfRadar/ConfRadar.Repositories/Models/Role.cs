using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Role
{
    public string Roleid { get; set; } = null!;

    public string Rolename { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
