namespace ConfRadar.Repositories.Models;

public partial class ConferenceCategory
{
    public string ConferenceCategoryId { get; set; } = null!;

    public string? ConferenceCategoryName { get; set; }

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();
}
