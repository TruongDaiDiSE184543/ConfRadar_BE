using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class GlobalStatus
{
    public string GlobalStatusId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Abstract> Abstracts { get; set; } = new List<Abstract>();

    public virtual ICollection<PresenterChangeRequest> PresenterChangeRequests { get; set; } = new List<PresenterChangeRequest>();

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();

    public virtual ICollection<RevisionPaper> RevisionPapers { get; set; } = new List<RevisionPaper>();

    public virtual ICollection<SessionChangeRequest> SessionChangeRequests { get; set; } = new List<SessionChangeRequest>();
}
