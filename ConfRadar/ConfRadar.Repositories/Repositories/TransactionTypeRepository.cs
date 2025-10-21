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
    public interface ITransactionTypeRepository
    {
        Task<TransactionType?> GetTransactionTypeByName(string transactionTypeName);
        Task<int> CreateMutipleTransactionTypesAsync(IEnumerable<TransactionType> transactionTypes);
    }
    public class TransactionTypeRepository : GenericRepository<TransactionType>,ITransactionTypeRepository
    {
        public TransactionTypeRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<TransactionType?> GetTransactionTypeByName(string transactionTypeName)
        {
            return await _context.TransactionTypes.FirstOrDefaultAsync(x => x.TypeName == transactionTypeName);
        }
        public async Task<int> CreateMutipleTransactionTypesAsync(IEnumerable<TransactionType> transactionTypes)
        {
            await _context.TransactionTypes.AddRangeAsync(transactionTypes);
            return await _context.SaveChangesAsync();
        }
    }
}
