using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperReviewerRepository
    {
        Task<int> CreatePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<int> UpdatePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<bool> DeletePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<PaperReviewer?> GetPaperReviewerByIdAsync(string paperReviewerId);
        Task<List<PaperReviewer>> GetAllPaperReviewersAsync();
        Task<PaperReviewer?> GetPaperReviewersByPaperIdAndUserIdAsync(string paperId, string userId);
        Task<List<PaperReviewer>> GetPaperReviewersByPaperIdAsync(string paperId);
    }
    public class PaperReviewerRepository : GenericRepository<PaperReviewer>, IPaperReviewerRepository
    {

        public PaperReviewerRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreatePaperReviewerAsync(PaperReviewer paperReviewer)
        {
            return await CreateAsync(paperReviewer);
        }

        public async Task<int> UpdatePaperReviewerAsync(PaperReviewer paperReviewer)
        {
            return await UpdateAsync(paperReviewer);
        }

        public async Task<bool> DeletePaperReviewerAsync(PaperReviewer paperReviewer)
        {
            return await RemoveAsync(paperReviewer);
        }

        public async Task<PaperReviewer?> GetPaperReviewerByIdAsync(string paperReviewerId)
        {
            return await GetByIdAsync(paperReviewerId);
        }

        public async Task<List<PaperReviewer>> GetAllPaperReviewersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<PaperReviewer>> GetPaperReviewersByPaperIdAsync(string paperId)
        {
            return await _context.PaperReviewers
                .Where(pr => pr.PaperId == paperId)
                .Include(pr => pr.User)
                .ToListAsync();
        }
        public async Task<PaperReviewer?> GetPaperReviewersByPaperIdAndUserIdAsync(string paperId,string userId)
        {
            return await _context.PaperReviewers
                .Include(pr=>pr.User)
                .Include(pr=>pr.Paper)
                .FirstOrDefaultAsync(pr => pr.PaperId == paperId && pr.UserId == userId);
                
        }



    }

}
