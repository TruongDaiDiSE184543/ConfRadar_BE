using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ITransactionRepository
    {
        Task<int> CreateTransactionAsync(Transaction transaction);
        Task<List<Transaction>> GetOwnTransactionByUserId(string userId);
        Task<int> CreateTransactionListAsync(List<Transaction> transactions);
    }
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateTransactionAsync(Transaction transaction)
        {
            return await CreateAsync(transaction);
        }
        public async Task<int> CreateTransactionListAsync(List<Transaction> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
            return await _context.SaveChangesAsync();
        }
        public async Task<List<Transaction>> GetOwnTransactionByUserId(string userId)
        {
            return await _context.Transactions
                .Include(x => x.PaymentMethod).
                Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
