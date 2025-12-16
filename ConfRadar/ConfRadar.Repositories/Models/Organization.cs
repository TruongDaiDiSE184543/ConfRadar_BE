namespace ConfRadar.Repositories.Models;

public partial class Organization
{
    public string OrganizationId { get; set; } = null!;

    public string? OrganizationDescription { get; set; }

    public string? OrganizationName { get; set; }

    public string? UserId { get; set; }

    public virtual User? User { get; set; }
}
