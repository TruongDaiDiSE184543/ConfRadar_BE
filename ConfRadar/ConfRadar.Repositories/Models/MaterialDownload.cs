namespace ConfRadar.Repositories.Models;

public partial class MaterialDownload
{
    public string MaterialDownloadId { get; set; } = null!;

    public string? ConferenceId { get; set; }

    public string? FileName { get; set; }

    public string? FileDescription { get; set; }

    public virtual Conference? Conference { get; set; }
}
