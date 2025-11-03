namespace ConfRadar.Repositories.Models;

public partial class PaperWaitList
{
    public string PaperWaitListId { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? NotifiedAt { get; set; }

    public string? WaitListStatusId { get; set; }

    public string? UserId { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual User? User { get; set; }

    public virtual WaitListStatus? WaitListStatus { get; set; }
}
