namespace ConfRadar.Repositories.Models;

public partial class RankingFileUrl
{
    public string RankingFileUrlId { get; set; } = null!;

    public string? FileUrl { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }
}
