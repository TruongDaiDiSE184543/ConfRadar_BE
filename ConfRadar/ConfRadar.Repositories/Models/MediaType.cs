namespace ConfRadar.Repositories.Models;

public partial class MediaType
{
    public string MediaTypeId { get; set; } = null!;

    public string? MediaTypeName { get; set; }

    public virtual ICollection<ConferenceMedium> ConferenceMedia { get; set; } = new List<ConferenceMedium>();
}
