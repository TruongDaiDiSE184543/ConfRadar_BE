using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferenceFeedback
{
    public string ConferenceFeedbackId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? ConferenceSessionId { get; set; }

    public int? Rating { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ConferenceSession? ConferenceSession { get; set; }

    public virtual User? User { get; set; }
}
