namespace ConfRadar.Repositories.Models;

public partial class RankingCategory
{
    public string RankingCategoryId { get; set; } = null!;

    public string? RankName { get; set; }

    public string? RankDescription { get; set; }

    public virtual ICollection<ResearchConferenceDetail> ResearchConferenceDetails { get; set; } = new List<ResearchConferenceDetail>();
}
