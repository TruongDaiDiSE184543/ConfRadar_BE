using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PresentAuthor
{
    public string? ConferenceSessionId { get; set; }

    public string? UserId { get; set; }

    public virtual ConferenceSession? ConferenceSession { get; set; }

    public virtual User? User { get; set; }
}
