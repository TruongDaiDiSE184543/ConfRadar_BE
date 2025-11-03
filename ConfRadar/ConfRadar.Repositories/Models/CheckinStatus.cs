namespace ConfRadar.Repositories.Models;

public partial class CheckinStatus
{
    public string CheckinStatusId { get; set; } = null!;

    public string? CheckinStatusName { get; set; }

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();
}
