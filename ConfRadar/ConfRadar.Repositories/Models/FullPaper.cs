using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class FullPaper
{
    public string FullPaperId { get; set; } = null!;

    public string? ReviewStatusId { get; set; }

    public string? FullPaperUrl { get; set; }

    public virtual ICollection<FullPaperReview> FullPaperReviews { get; set; } = new List<FullPaperReview>();

    public virtual ReviewStatus? ReviewStatus { get; set; }
}
