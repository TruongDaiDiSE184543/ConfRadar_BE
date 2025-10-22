namespace ConfRadar.Repositories.Models;

public partial class ConferencePrice
{
    public string ConferencePriceId { get; set; } = null!;

    public decimal? TicketPrice { get; set; }

    public string? TicketName { get; set; }

    public string? TicketDescription { get; set; }

    public decimal? ActualPrice { get; set; }

    public string? PricePhaseId { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual PricePhase? PricePhase { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
