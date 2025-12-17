using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class CameraReady
{
    public string CameraReadyId { get; set; } = null!;

    public string? CameraReadyUrl { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewAt { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
