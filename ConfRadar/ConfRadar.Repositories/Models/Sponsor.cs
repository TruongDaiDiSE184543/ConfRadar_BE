using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Sponsor
{
    public string SponsorId { get; set; } = null!;

    public string? Name { get; set; }

    public string? ImageUrl { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }
}
