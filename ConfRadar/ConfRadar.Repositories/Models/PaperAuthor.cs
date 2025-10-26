using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PaperAuthor
{
    public string? UserId { get; set; }

    public string? PaperId { get; set; }

    public bool? IsPresenter { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual User? User { get; set; }
}
