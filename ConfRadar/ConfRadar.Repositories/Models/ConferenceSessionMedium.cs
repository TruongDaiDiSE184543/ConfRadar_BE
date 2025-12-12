namespace ConfRadar.Repositories.Models;

public partial class ConferenceSessionMedium
{
    public string ConferenceSessionMediaId { get; set; } = null!;

    public string? MediaUrl { get; set; }

    public string? ConferenceSessionId { get; set; }

    public virtual ConferenceSession? ConferenceSession { get; set; }
}
