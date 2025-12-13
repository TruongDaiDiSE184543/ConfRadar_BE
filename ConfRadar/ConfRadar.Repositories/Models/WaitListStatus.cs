using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class WaitListStatus
{
    public string WaitListStatusId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<PaperWaitList> PaperWaitLists { get; set; } = new List<PaperWaitList>();
}
