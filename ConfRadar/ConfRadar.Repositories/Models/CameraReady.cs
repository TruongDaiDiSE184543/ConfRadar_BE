namespace ConfRadar.Repositories.Models;

public partial class CameraReady
{
    public string CameraReadyId { get; set; } = null!;

    public string? GlobalStatusId { get; set; }

    public string? CameraReadyUrl { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewAt { get; set; }

    public string? Reason { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
