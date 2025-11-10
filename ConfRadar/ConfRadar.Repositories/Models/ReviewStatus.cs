namespace ConfRadar.Repositories.Models;

public partial class ReviewStatus
{
    public string ReviewStatusId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<FullPaperReview> FullPaperReviews { get; set; } = new List<FullPaperReview>();

    public virtual ICollection<FullPaper> FullPapers { get; set; } = new List<FullPaper>();
}
