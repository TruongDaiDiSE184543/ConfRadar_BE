namespace ConfRadar.Repositories.Models;

public partial class RevisionSubmissionFeedback
{
    public string RevisionSubmissionFeedbackId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Feedback { get; set; }

    public string? Response { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? RevisionPaperSubmissionId { get; set; }

    public virtual RevisionPaperSubmission? RevisionPaperSubmission { get; set; }

    public virtual User? User { get; set; }
}
