using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ResearchConferenceDetail
{
    public string ConferenceId { get; set; } = null!;

    public int? NumberPaperAccept { get; set; }

    public int? RevisionAttemptAllowed { get; set; }

    public string? RankingDescription { get; set; }

    public bool? AllowListener { get; set; }

    public string? RankValue { get; set; }

    public int? RankYear { get; set; }

    public decimal? SubmitPaperFee { get; set; }

    public string? RankingCategoryId { get; set; }

    public string? PublisherId { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual Publisher? Publisher { get; set; }

    public virtual RankingCategory? RankingCategory { get; set; }
}
