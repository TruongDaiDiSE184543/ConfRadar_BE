using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ResearchConferencePhase
{
    public string ResearchConferencePhaseId { get; set; } = null!;

    public string? ConferenceId { get; set; }

    public DateOnly? RegistrationStartDate { get; set; }

    public DateOnly? RegistrationEndDate { get; set; }

    public DateOnly? FullPaperStartDate { get; set; }

    public DateOnly? FullPaperEndDate { get; set; }

    public DateOnly? ReviewStartDate { get; set; }

    public DateOnly? ReviewEndDate { get; set; }

    public DateOnly? ReviseStartDate { get; set; }

    public DateOnly? ReviseEndDate { get; set; }

    public DateOnly? CameraReadyStartDate { get; set; }

    public DateOnly? CameraReadyEndDate { get; set; }

    public bool? IsWaitlist { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? AbstractDecideStatusStart { get; set; }

    public DateOnly? AbstractDecideStatusEnd { get; set; }

    public DateOnly? FullPaperDecideStatusStart { get; set; }

    public DateOnly? FullPaperDecideStatusEnd { get; set; }

    public DateOnly? RevisionPaperReviewStart { get; set; }

    public DateOnly? RevisionPaperReviewEnd { get; set; }

    public DateOnly? RevisionPaperDecideStatusStart { get; set; }

    public DateOnly? RevisionPaperDecideStatusEnd { get; set; }

    public DateOnly? CameraReadyDecideStatusStart { get; set; }

    public DateOnly? CameraReadyDecideStatusEnd { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<PricePhase> PricePhases { get; set; } = new List<PricePhase>();

    public virtual ICollection<RevisionRoundDeadline> RevisionRoundDeadlines { get; set; } = new List<RevisionRoundDeadline>();
}
