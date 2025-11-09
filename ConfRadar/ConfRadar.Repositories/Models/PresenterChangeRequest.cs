using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PresenterChangeRequest
{
    public string PresenterChangeRequestId { get; set; } = null!;

    public string? TicketId { get; set; }

    public string? RequestedById { get; set; }

    public string? NewPresenterId { get; set; }

    public string? GlobalStatusId { get; set; }

    public string? PaperId { get; set; }

    public string? Reason { get; set; }

    public DateTime? RequestAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual GlobalStatus? GlobalStatus { get; set; }

    public virtual User? NewPresenter { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual User? RequestedBy { get; set; }
}
