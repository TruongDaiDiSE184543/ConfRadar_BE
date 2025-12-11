using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ConferencePrice
{
    public string ConferencePriceId { get; set; } = null!;

    public decimal? TicketPrice { get; set; }

    public string? TicketName { get; set; }

    public string? TicketDescription { get; set; }

    public bool? IsAuthor { get; set; }

    public int? TotalSlot { get; set; }

    public int? AvailableSlot { get; set; }

    public string? ConferenceId { get; set; }

    public bool? IsPublish { get; set; }

    public string? PublisherId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ICollection<PricePhase> PricePhases { get; set; } = new List<PricePhase>();

    public virtual Publisher? Publisher { get; set; }
}
