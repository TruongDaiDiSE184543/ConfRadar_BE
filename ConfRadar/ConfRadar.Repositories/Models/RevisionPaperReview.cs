namespace ConfRadar.Repositories.Models;

public partial class RevisionPaperReview
{
    public string RevisionPaperReviewId { get; set; } = null!;

    public string? GlobalStatusId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? FeedbackMaterialUrl { get; set; }

    public string? ReviewerId { get; set; }

    public string? RevisionPaperId { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual User? Reviewer { get; set; }

    public virtual RevisionPaper? RevisionPaper { get; set; }
}
