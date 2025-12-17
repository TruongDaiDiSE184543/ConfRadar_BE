using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class WalletTransaction
{
    public string WalletTransactionId { get; set; } = null!;

    public string? WalletId { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionType { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
