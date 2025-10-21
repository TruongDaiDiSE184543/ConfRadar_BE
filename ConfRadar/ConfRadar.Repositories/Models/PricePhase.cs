namespace ConfRadar.Repositories.Models;

public partial class PricePhase
{
    public string PricePhaseId { get; set; } = null!;

    public string? Name { get; set; }

    public DateOnly? EarlierBirdEndInterval { get; set; }

    public int? PercentForEarly { get; set; }

    public DateOnly? StandardEndInterval { get; set; }

    public DateOnly? LateEndInterval { get; set; }

    public int? PercentForEnd { get; set; }

    public virtual ICollection<ConferencePrice> ConferencePrices { get; set; } = new List<ConferencePrice>();
}
