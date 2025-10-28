using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{

    public interface IRevisionPaperRepository
    {
        Task<int> CreateMultipleRevisionPapersAsync(List<RevisionPaper> revisionPapers);
        Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<RevisionPaper?> GetRevisionByIdAsync(string revisionPaperId);
        Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper);
    }
    public class RevisionPaperRepository : GenericRepository<RevisionPaper>, IRevisionPaperRepository
    {
        public RevisionPaperRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateMultipleRevisionPapersAsync(List<RevisionPaper> revisionPapers)
        {
            await _context.RevisionPapers.AddRangeAsync(revisionPapers);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await CreateAsync(revisionPaper);
        }


        public async Task<RevisionPaper?> GetRevisionByIdAsync(string revisionPaperId)
        {
            return await _context.RevisionPapers
                .Include(rp => rp.RevisionPaperSubmissions)
                .FirstOrDefaultAsync(rp => rp.RevisionPaperId == revisionPaperId);
        }
        public async Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await UpdateAsync(revisionPaper);
        }

        public async Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper)
        {
            return await RemoveAsync(revisionPaper);
        }

        public async Task<List<RevisionPaper>> GetAllRevisionPapersAsync()
        {
            return await GetAllAsync();
        }
    }
}
