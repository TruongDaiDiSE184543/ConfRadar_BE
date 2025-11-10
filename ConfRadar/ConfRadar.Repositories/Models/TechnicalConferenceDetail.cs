namespace ConfRadar.Repositories.Models;

public partial class TechnicalConferenceDetail
{
    public string ConferenceId { get; set; } = null!;

    public string? TargetAudience { get; set; }

    public int? Commission { get; set; }

    public string? ContractUrl { get; set; }

    public virtual Conference Conference { get; set; } = null!;
}
