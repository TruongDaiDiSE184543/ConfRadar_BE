using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class PaymentMethod
{
    public string PaymentMethodId { get; set; } = null!;

    public string? MethodName { get; set; }

    public string? MethodDescription { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
