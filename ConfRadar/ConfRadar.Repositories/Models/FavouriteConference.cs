namespace ConfRadar.Repositories.Models;

public partial class FavouriteConference
{
    public string UserId { get; set; } = null!;

    public string ConferenceId { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
