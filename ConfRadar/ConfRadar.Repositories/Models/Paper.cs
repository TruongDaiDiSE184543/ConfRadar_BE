namespace ConfRadar.Repositories.Models;

public partial class Paper
{
    public string PaperId { get; set; } = null!;

    public string? FullPaperId { get; set; }

    public string? RevisionPaperId { get; set; }

    public string? CameraReadyId { get; set; }

    public string? AbstractId { get; set; }

    public string? ConferenceId { get; set; }

    public string? PaperPhaseId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public virtual Abstract? Abstract { get; set; }

    public virtual CameraReady? CameraReady { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual FullPaper? FullPaper { get; set; }

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual PaperPhase? PaperPhase { get; set; }

    public virtual ICollection<PaperReviewer> PaperReviewers { get; set; } = new List<PaperReviewer>();

    public virtual ICollection<PresentAuthor> PresentAuthors { get; set; } = new List<PresentAuthor>();

    public virtual RevisionPaper? RevisionPaper { get; set; }
}
