using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferenceTimeline
{
    public string ConferenceTimelineId { get; set; } = null!;

    public string? ConferenceId { get; set; }

    public DateOnly? ChangeDate { get; set; }

    public string? PreviousStatusId { get; set; }

    public string? AfterwardStatusId { get; set; }

    public string? Reason { get; set; }

    public virtual ConferenceStatus? AfterwardStatus { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ConferenceStatus? PreviousStatus { get; set; }
}
