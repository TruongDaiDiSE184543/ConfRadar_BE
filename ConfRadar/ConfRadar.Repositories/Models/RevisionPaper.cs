namespace ConfRadar.Repositories.Models;

public partial class RevisionPaper
{
    public string RevisionPaperId { get; set; } = null!;

    public int? RevisionRound { get; set; }

    public string? GlobalStatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewAt { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<RevisionPaperReview> RevisionPaperReviews { get; set; } = new List<RevisionPaperReview>();

    public virtual ICollection<RevisionPaperSubmission> RevisionPaperSubmissions { get; set; } = new List<RevisionPaperSubmission>();
}
