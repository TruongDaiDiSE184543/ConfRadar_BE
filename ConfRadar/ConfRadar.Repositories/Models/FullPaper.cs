using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class FullPaper
{
    public string FullPaperId { get; set; } = null!;

    public string? ReviewStatusId { get; set; }

    public string? FullPaperUrl { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewAt { get; set; }

    public virtual ICollection<FullPaperReview> FullPaperReviews { get; set; } = new List<FullPaperReview>();

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ReviewStatus? ReviewStatus { get; set; }
}
