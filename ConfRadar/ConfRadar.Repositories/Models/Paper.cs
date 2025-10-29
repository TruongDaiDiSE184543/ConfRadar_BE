using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class Paper
{
    public string PaperId { get; set; } = null!;

    public string? PresenterId { get; set; }

    public string? FullPaperId { get; set; }

    public string? RevisionPaperId { get; set; }

    public string? CameraReadyId { get; set; }

    public string? AbstractId { get; set; }

    public string? ConferenceId { get; set; }

    public string? PaperPhaseId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual CameraReady? CameraReady { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual PaperPhase? PaperPhase { get; set; }

    public virtual User? Presenter { get; set; }
}
