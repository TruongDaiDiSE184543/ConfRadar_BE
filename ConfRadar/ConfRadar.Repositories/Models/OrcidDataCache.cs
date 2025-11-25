namespace ConfRadar.Repositories.Models;

public partial class OrcidDataCache
{
    public string OrcidDataCacheId { get; set; } = null!;

    public string AcademicProfileId { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public string? JsonContent { get; set; }

    public DateTime LastSyncedAt { get; set; }

    public virtual AcademicProfile AcademicProfile { get; set; } = null!;
}
