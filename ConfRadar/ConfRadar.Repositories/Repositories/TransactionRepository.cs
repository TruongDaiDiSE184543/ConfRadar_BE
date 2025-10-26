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
        public async Task<List<Transaction>> GetOwnTransactionByUserId(string userId)
        {
            return await _context.Transactions
                .Include(x=>x.PaymentMethod).
                Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
