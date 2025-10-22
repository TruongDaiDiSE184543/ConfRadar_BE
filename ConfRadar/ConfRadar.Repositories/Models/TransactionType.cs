namespace ConfRadar.Repositories.Models;

public partial class TransactionType
{
    public string TransactionTypeId { get; set; } = null!;

    public string? TypeName { get; set; }

    public string? TypeDescription { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
