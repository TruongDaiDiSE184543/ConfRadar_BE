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
    public interface IGeneralFAQRepository
    {
        Task<GeneralFaq?> GetByIdAsync(string generalFAQId);
        Task<List<GeneralFaq>> GetAllGeneralFAQAsync();
        Task<int> CreateGeneralFAQAsync(GeneralFaq generalFAQ);
        Task<int> CreateMultipleAsync(List<GeneralFaq> generalFAQs);
    }
    public class GeneralFAQRepository : GenericRepository<GeneralFaq>, IGeneralFAQRepository
    {
        public GeneralFAQRepository(ConfRadarDbContext context): base(context)
        {
        }

        public async Task<GeneralFaq?> GetByIdAsync(string generalFAQId)
        {
            return await _context.GeneralFaqs
                .FirstOrDefaultAsync(f => f.GeneralFaqid == generalFAQId);
        }

        public async Task<List<GeneralFaq>> GetAllGeneralFAQAsync()
        {
            return await _context.GeneralFaqs
                .OrderBy(f => f.GeneralFaqid)
                .ToListAsync();
        }

        public async Task<int> CreateGeneralFAQAsync(GeneralFaq generalFAQ)
        {
            await _context.GeneralFaqs.AddAsync(generalFAQ);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateMultipleAsync(List<GeneralFaq> generalFAQs)
        {
            await _context.GeneralFaqs.AddRangeAsync(generalFAQs);
            return await _context.SaveChangesAsync();
        }

       
    }
}
