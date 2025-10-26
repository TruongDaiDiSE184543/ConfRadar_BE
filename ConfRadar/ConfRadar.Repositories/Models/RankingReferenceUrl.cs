using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RankingReferenceUrl
{
    public string ReferenceUrlId { get; set; } = null!;

    public string? ConferenceId { get; set; }

    public string? ReferenceUrl { get; set; }

    public virtual Conference? Conference { get; set; }
}
