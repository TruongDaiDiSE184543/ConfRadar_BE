using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RefundPolicy
{
    public string RefundPolicyId { get; set; } = null!;

    public string? ConferenceId { get; set; }

    public string? PricePhaseId { get; set; }

    public int? PercentRefund { get; set; }

    public DateOnly? RefundDeadline { get; set; }

    public int? RefundOrder { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual PricePhase? PricePhase { get; set; }
}
