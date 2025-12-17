namespace ConfRadar.Repositories.Models;

public partial class PaperAuthor
{
    public string UserId { get; set; } = null!;

    public string PaperId { get; set; } = null!;

    public bool? IsPresenter { get; set; }

    public bool? IsRootAuthor { get; set; }

    public virtual Paper Paper { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
