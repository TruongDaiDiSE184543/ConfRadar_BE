namespace ConfRadar.Repositories.Models;

public partial class AuditLogCategory
{
    public string CategoryId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
