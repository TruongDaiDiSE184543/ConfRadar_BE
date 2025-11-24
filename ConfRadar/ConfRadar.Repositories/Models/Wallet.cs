namespace ConfRadar.Repositories.Models;

public partial class Wallet
{
    public string WalletId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public decimal? Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
