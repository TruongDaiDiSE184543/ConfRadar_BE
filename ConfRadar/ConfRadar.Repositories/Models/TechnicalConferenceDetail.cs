using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class TechnicalConferenceDetail
{
    public string ConferenceId { get; set; } = null!;

    public string? TargetAudience { get; set; }

    public virtual Conference Conference { get; set; } = null!;
}
