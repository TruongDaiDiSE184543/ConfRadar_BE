namespace ConfRadar.Repositories.Models;

public partial class ReportFeedback
{
    public string ReportId { get; set; } = null!;

    public string? ReportSubject { get; set; }

    public string? Reason { get; set; }

    public string? AdminId { get; set; }

    public virtual User? Admin { get; set; }
}
