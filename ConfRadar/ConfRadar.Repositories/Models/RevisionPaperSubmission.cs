using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RevisionPaperSubmission
{
    public string RevisionPaperSubmissionId { get; set; } = null!;

    public string? RevisionPaperUrl { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? RevisionPaperId { get; set; }

    public string? RevisionDeadlineRoundId { get; set; }

    public virtual RevisionRoundDeadline? RevisionDeadlineRound { get; set; }

    public virtual RevisionPaper? RevisionPaper { get; set; }

    public virtual ICollection<RevisionSubmissionFeedback> RevisionSubmissionFeedbacks { get; set; } = new List<RevisionSubmissionFeedback>();
}
