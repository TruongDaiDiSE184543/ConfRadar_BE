using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Policy
{
    public string PolicyId { get; set; } = null!;

    public string? PolicyName { get; set; }

    public string? Description { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }
}
