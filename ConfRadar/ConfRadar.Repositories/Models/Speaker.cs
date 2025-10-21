using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Speaker
{
    public string ConferenceSessionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ConferenceSession ConferenceSession { get; set; } = null!;
}
