namespace ConfRadar.Repositories.Models;

public partial class Abstract
{
    public string AbstractId { get; set; } = null!;

    public string? GlobalStatusId { get; set; }

    public string? AbstractUrl { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }
}
