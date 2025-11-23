using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class UserCheckIn
{
    public string UserCheckinId { get; set; } = null!;

    public bool? IsPresenter { get; set; }

    public string? CheckinStatusId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? UserId { get; set; }

    public string? TicketId { get; set; }

    public string? ConferenceSessionId { get; set; }

    public string? QrUrl { get; set; }

    public virtual CheckinStatus? CheckinStatus { get; set; }

    public virtual ConferenceSession? ConferenceSession { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual User? User { get; set; }
}
