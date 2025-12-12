namespace ConfRadar.Repositories.Models;

public partial class Publisher
{
    public string PublisherId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string PaperFormat { get; set; } = null!;

    public string? LinkTemplate { get; set; }

    public virtual ICollection<ResearchConferenceDetail> ResearchConferenceDetails { get; set; } = new List<ResearchConferenceDetail>();
}
