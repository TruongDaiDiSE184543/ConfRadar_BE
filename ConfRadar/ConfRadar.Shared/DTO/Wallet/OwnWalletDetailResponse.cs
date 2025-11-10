using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Wallet
{
    public class OwnWalletDetailResponse
    {
        public string? WalletId { get; set; } 
        public decimal? Balance { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OwnWalletTransactionDetailResponse> WalletTransactions { get; set; } = new List<OwnWalletTransactionDetailResponse>();
    }
    public class OwnWalletTransactionDetailResponse
    {
        public string? WalletTransactionId { get; set; } 
        public string? WalletId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionType { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
