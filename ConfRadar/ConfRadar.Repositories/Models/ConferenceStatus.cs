using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferenceStatus
{
    public string ConferenceStatusId { get; set; } = null!;

    public string? ConferenceStatusName { get; set; }

    public virtual ICollection<ConferenceTimeline> ConferenceTimelineAfterwardStatuses { get; set; } = new List<ConferenceTimeline>();

    public virtual ICollection<ConferenceTimeline> ConferenceTimelinePreviousStatuses { get; set; } = new List<ConferenceTimeline>();
}
