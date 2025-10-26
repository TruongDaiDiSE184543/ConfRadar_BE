using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PaperPhase
{
    public string PaperPhaseId { get; set; } = null!;

    public string? PhaseName { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
