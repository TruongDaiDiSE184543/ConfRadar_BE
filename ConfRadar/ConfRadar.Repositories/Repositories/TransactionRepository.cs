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
        Task<Transaction> GetRefundTransactionByTicket(string ticketId);
        Task<Transaction> GetNotRefundTransactionBytTicket(string ticketId);
        IQueryable<Transaction> TransactionHistory(string confId);
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

        public async Task<Transaction> GetRefundTransactionByTicket(string ticketId)
        {
            return await _context.Transactions.Include(t => t.Ticket)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId && t.IsRefunded == true);
        }

        public async Task<Transaction> GetNotRefundTransactionBytTicket(string ticketId)
        {
            return await _context.Transactions.Include(t => t.Ticket)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId && t.IsRefunded == false);
        }

        public IQueryable<Transaction> TransactionHistory(string confId)
        {
            return _context.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.PaymentMethod)
                .Include(t => t.RefundRequests)
                    .ThenInclude(rr => rr.GlobalStatus)
                .Include(t => t.Ticket)
                    .ThenInclude(tick => tick.PricePhase)
                        .ThenInclude(pp => pp.ConferencePrice)
                .Where(t => t.Ticket.PricePhase.ConferencePrice.ConferenceId == confId);
        }
    }
}
