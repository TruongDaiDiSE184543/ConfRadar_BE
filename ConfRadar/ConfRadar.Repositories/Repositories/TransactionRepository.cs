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
        public Task<List<Transaction>> GetOwnTransactionByUserId(string userId)
        {
            return _context.Transactions.Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
