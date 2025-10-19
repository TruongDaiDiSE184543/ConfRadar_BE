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
    public interface ITransactionStatusRepository
    {
        Task<TransactionStatus?> GetTransactionStatusByName(string transactionStatusName);
        Task<int> CreateMutipleTransactionStatusesAsync(IEnumerable<TransactionStatus> transactionStatuses);
    }
    public class TransactionStatusRepository : GenericRepository<TransactionStatus>, ITransactionStatusRepository
    {
        public TransactionStatusRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<TransactionStatus?> GetTransactionStatusByName(string transactionStatusName)
        {
            return await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.StatusName == transactionStatusName);
        }
        public async Task<int> CreateMutipleTransactionStatusesAsync(IEnumerable<TransactionStatus> transactionStatuses)
        {
            await _context.TransactionStatuses.AddRangeAsync(transactionStatuses);
            return await _context.SaveChangesAsync();
        }
    }
}
