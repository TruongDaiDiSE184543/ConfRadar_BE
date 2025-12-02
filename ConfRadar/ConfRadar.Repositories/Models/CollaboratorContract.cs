using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class CollaboratorContract
{
    public string CollaboratorContractId { get; set; } = null!;

    public string? UserId { get; set; }

    public bool? IsSponsorStep { get; set; }

    public bool? IsMediaStep { get; set; }

    public bool? IsPolicyStep { get; set; }

    public bool? IsSessionStep { get; set; }

    public bool? IsPriceStep { get; set; }

    public bool? IsTicketSelling { get; set; }

    public bool? IsClosed { get; set; }

    public DateOnly? SignDay { get; set; }

    public DateOnly? FinalizePaymentDate { get; set; }

    public int? Commission { get; set; }

    public string? ContractUrl { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual User? User { get; set; }
}
