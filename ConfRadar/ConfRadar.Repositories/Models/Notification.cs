namespace ConfRadar.Repositories.Models;

public partial class Notification
{
    public string NotificationId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? ReadStatus { get; set; }

    public virtual User? User { get; set; }
}
