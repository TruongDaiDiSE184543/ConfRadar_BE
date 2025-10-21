namespace ConfRadar.Repositories.Models;

public partial class ConferenceMedium
{
    public string ConferenceMediaId { get; set; } = null!;

    public string? ConferenceMediaUrl { get; set; }

    public string? ConferenceId { get; set; }

    public string? MediaTypeId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual MediaType? MediaType { get; set; }
}
