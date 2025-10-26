using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PaperReviewer
{
    public string? PaperId { get; set; }

    public string? UserId { get; set; }

    public bool? IsHeadReviewer { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual User? User { get; set; }
}
