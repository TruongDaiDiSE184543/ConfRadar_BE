using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferenceSession
{
    public string ConferenceSessionId { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateOnly? SessionDate { get; set; }

    public string? ConferenceId { get; set; }

    public string? RoomId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ICollection<ConferenceFeedback> ConferenceFeedbacks { get; set; } = new List<ConferenceFeedback>();

    public virtual ICollection<ConferenceSessionMedium> ConferenceSessionMedia { get; set; } = new List<ConferenceSessionMedium>();

    public virtual ICollection<PresentAuthor> PresentAuthors { get; set; } = new List<PresentAuthor>();

    public virtual Room? Room { get; set; }

    public virtual ICollection<SessionChangeRequest> SessionChangeRequests { get; set; } = new List<SessionChangeRequest>();

    public virtual ICollection<Speaker> Speakers { get; set; } = new List<Speaker>();

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();
}
