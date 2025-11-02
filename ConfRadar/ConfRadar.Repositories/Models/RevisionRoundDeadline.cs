using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class RevisionRoundDeadline
{
    public string RevisionRoundDeadlineId { get; set; } = null!;

    public DateOnly? EndSubmissionDate { get; set; }

    public int? RoundNumber { get; set; }

    public string? ResearchConferencePhaseId { get; set; }

    public DateOnly? StartSubmissionDate { get; set; }

    public virtual ResearchConferencePhase? ResearchConferencePhase { get; set; }

    public virtual ICollection<RevisionPaperSubmission> RevisionPaperSubmissions { get; set; } = new List<RevisionPaperSubmission>();
}
