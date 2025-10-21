namespace ConfRadar.Repositories.Models;

public partial class FavouriteConference
{
    public string FavouriteConferenceId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? ConferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual User? User { get; set; }
}
