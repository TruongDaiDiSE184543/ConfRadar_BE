namespace ConfRadar.Repositories.Models;

public partial class FullPaperReview
{
    public string FullPaperReviewId { get; set; } = null!;

    public string? ReviewStatusId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? FeedbackMaterialUrl { get; set; }

    public string? FullPaperId { get; set; }

    public string? ReviewerId { get; set; }

    public virtual FullPaper? FullPaper { get; set; }

    public virtual ReviewStatus? ReviewStatus { get; set; }

    public virtual User? Reviewer { get; set; }
}
