using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Room
{
    public string RoomId { get; set; } = null!;

    public string? Number { get; set; }

    public string? DisplayName { get; set; }

    public string? DestinationId { get; set; }

    public virtual ICollection<ConferenceSession> ConferenceSessions { get; set; } = new List<ConferenceSession>();

    public virtual Destination? Destination { get; set; }
}
