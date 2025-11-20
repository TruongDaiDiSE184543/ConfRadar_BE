using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PresentAuthor
{
    public string ConferenceSessionId { get; set; } = null!;

    public string PaperId { get; set; } = null!;

    public DateTime? AssignedAt { get; set; }

    public virtual ConferenceSession ConferenceSession { get; set; } = null!;

    public virtual Paper Paper { get; set; } = null!;
}
