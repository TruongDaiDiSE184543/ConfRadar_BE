namespace ConfRadar.Repositories.Models;

public partial class TransactionStatus
{
    public string TransactionStatusId { get; set; } = null!;

    public string? StatusName { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
