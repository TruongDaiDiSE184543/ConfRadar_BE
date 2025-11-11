using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PricePhase
{
    public string PricePhaseId { get; set; } = null!;

    public string? PhaseName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? ApplyPercent { get; set; }

    public int? TotalSlot { get; set; }

    public int? AvailableSlot { get; set; }

    public string? ConferencePriceId { get; set; }

    public virtual ConferencePrice? ConferencePrice { get; set; }

    public virtual ICollection<RefundPolicy> RefundPolicies { get; set; } = new List<RefundPolicy>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
