using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class AcademicProfile
{
    public string AcademicProfileId { get; set; } = null!;

    public string? UserId { get; set; }

    public virtual User? User { get; set; }
}
