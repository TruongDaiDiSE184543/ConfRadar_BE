using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RevisionRoundDeadline
{
    public string RevisionRoundDeadlineId { get; set; } = null!;

    public DateOnly? StartSubmissionDate { get; set; }

    public DateOnly? EndSubmissionDate { get; set; }

    public int? RoundNumber { get; set; }

    public string? ResearchConferencePhaseId { get; set; }

    public virtual ResearchConferencePhase? ResearchConferencePhase { get; set; }

    public virtual ICollection<RevisionPaperSubmission> RevisionPaperSubmissions { get; set; } = new List<RevisionPaperSubmission>();

    public virtual ICollection<RevisionPaper> RevisionPapers { get; set; } = new List<RevisionPaper>();
}
