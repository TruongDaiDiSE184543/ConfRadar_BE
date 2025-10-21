using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferenceSession
{
    public string ConferenceSessionId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ConferenceId { get; set; }

    public string? RoomId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual Room? Room { get; set; }

    public virtual Speaker? Speaker { get; set; }
}
