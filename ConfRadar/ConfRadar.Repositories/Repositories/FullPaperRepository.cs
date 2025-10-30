using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ConfRadar.Repositories.Repositories
{
    public class FullPaperRepository : GenericRepository<FullPaper>, IFullPaperRepository
    {
        public FullPaperRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateFullPaperAsync(FullPaper fullPaper)
        {
            return await CreateAsync(fullPaper);
        }

        public async Task<int> UpdateFullPaperAsync(FullPaper fullPaper)
        {
            return await UpdateAsync(fullPaper);
        }

        public async Task<bool> DeleteFullPaperAsync(FullPaper fullPaper)
        {
            return await RemoveAsync(fullPaper);
        }

        public async Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId)
        {
            return await _context.FullPapers.Where(fp => fp.FullPaperId == fullPaperId).Include(fp => fp.ReviewStatus).FirstOrDefaultAsync();
        }

        public async Task<List<FullPaper>> GetAllFullPapersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<FullPaper>> GetFullPaperByStatusName(string status)
        {
            return await _context.FullPapers.Where(fp => fp.ReviewStatus.Name == status).ToListAsync();
        }

      
    }
}