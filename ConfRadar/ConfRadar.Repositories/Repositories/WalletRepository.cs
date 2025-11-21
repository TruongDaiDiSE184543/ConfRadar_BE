using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.Wallet;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IWalletRepository
    {
        Task<int> CreateWalletAsync(Wallet wallet);
        Task<int> UpdateWalletAsync(Wallet wallet);
        Task<Wallet?> GetWalletByUserIdAsync(string userId);
        Task<Wallet?> GetWalletByIdAsync(string walletId);
        Task<List<Wallet>> GetAllWalletsAsync();
        Task<OwnWalletDetailResponse?> ViewOwnWallet(string userId);
    }
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateWalletAsync(Wallet wallet)
        {
            return await CreateAsync(wallet);
        }

        public async Task<int> UpdateWalletAsync(Wallet wallet)
        {
            return await UpdateAsync(wallet);
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
        {
            return await _context.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet?> GetWalletByIdAsync(string walletId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.WalletId == walletId);
        }

        public async Task<List<Wallet>> GetAllWalletsAsync()
        {
            return await _context.Wallets.ToListAsync();
        }

        public async Task<OwnWalletDetailResponse?> ViewOwnWallet(string userId)
        {
            var walletDetail = await _context.Wallets
                .Include(w => w.WalletTransactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);
            if (walletDetail == null)
            {
                return null;
            }
            return new OwnWalletDetailResponse()
            {
                WalletId = walletDetail.WalletId,
                Balance = walletDetail.Balance,
                CreatedAt = walletDetail.CreatedAt,
                UpdatedAt = walletDetail.UpdatedAt,
                WalletTransactions = walletDetail.WalletTransactions.Select(wt => new OwnWalletTransactionDetailResponse()
                {
                    WalletTransactionId = wt.WalletTransactionId,
                    WalletId = wt.WalletId,
                    Amount = wt.Amount,
                    TransactionType = wt.TransactionType,
                    Description = wt.Description,
                    CreatedAt = wt.CreatedAt,
                }).ToList()
            };
        }
    }
}
