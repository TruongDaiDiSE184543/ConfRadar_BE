namespace ConfRadar.Repositories.Models;

public partial class Speaker
{
    public string SpeakerId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public string? ConferenceSessionId { get; set; }

    public virtual ConferenceSession? ConferenceSession { get; set; }
}
