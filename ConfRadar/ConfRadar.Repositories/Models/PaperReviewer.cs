namespace ConfRadar.Repositories.Models;

public partial class PaperReviewer
{
    public string PaperId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool? IsHeadReviewer { get; set; }

    public virtual Paper Paper { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
