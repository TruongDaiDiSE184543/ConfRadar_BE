using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class ReviewerContract
{
    public string ReviewerContractId { get; set; } = null!;

    public string? UserId { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? SignDay { get; set; }

    public DateOnly? ExpireDay { get; set; }

    public decimal? Wage { get; set; }

    public string? ContractUrl { get; set; }

    public string? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual User? User { get; set; }
}
