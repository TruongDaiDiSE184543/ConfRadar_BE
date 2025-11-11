using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task<int> CreateWalletTransactionAsync(WalletTransaction walletTransaction);
        Task<int> UpdateWalletTransactionAsync(WalletTransaction walletTransaction);
        Task<WalletTransaction?> GetWalletTransactionByIdAsync(string walletTransactionId);
        Task<List<WalletTransaction>> GetWalletTransactionsByWalletIdAsync(string walletId);
        Task<List<WalletTransaction>> GetAllWalletTransactionsAsync();
    }
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateWalletTransactionAsync(WalletTransaction walletTransaction)
        {
            return await CreateAsync(walletTransaction);
        }

        public async Task<int> UpdateWalletTransactionAsync(WalletTransaction walletTransaction)
        {
            return await UpdateAsync(walletTransaction);
        }

        public async Task<WalletTransaction?> GetWalletTransactionByIdAsync(string walletTransactionId)
        {
            return await _context.WalletTransactions
                .FirstOrDefaultAsync(wt => wt.WalletTransactionId == walletTransactionId);
        }

        public async Task<List<WalletTransaction>> GetWalletTransactionsByWalletIdAsync(string walletId)
        {
            return await _context.WalletTransactions
                .Where(wt => wt.WalletId == walletId)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WalletTransaction>> GetAllWalletTransactionsAsync()
        {
            return await _context.WalletTransactions.ToListAsync();
        }
    }

}
