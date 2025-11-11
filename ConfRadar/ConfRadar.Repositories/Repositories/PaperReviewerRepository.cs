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
        Task<PaperReviewer?> GetPaperReviewersByPaperIdAndUserIdAsync(string? userId, string? paperId);
        Task<List<PaperReviewer>> GetAllPaperReviewersAsync();
        Task<List<PaperReviewer>> GetPaperReviewersByPaperIdAsync(string paperId);
        Task<List<PaperReviewer>> GetPaperReviewersByUserIdAsync(string userId);
        Task<List<PaperReviewer>> GetHeadReviewersByPaperIdAsync(string paperId);
        Task<List<PaperReviewer>> GetPaperReviewersByUserIdAndConferenceIdAsync(string userId, string conferenceId);
        Task<List<Paper>> getAllAssignedPapers(string userId);
    }
    public class PaperReviewerRepository : GenericRepository<PaperReviewer>, IPaperReviewerRepository
    {
        public PaperReviewerRepository(ConfRadarDbContext context) : base(context) { }

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

        public async Task<PaperReviewer?> GetPaperReviewersByPaperIdAndUserIdAsync(string? userId, string? paperId)
        {
            return await _context.Set<PaperReviewer>()
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.PaperId == paperId);
        }

        public async Task<List<PaperReviewer>> GetAllPaperReviewersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<Paper>> getAllAssignedPapers(string userId)
        {
            return await _context.PaperReviewers
         .AsNoTracking()
         .Where(pr => pr.UserId == userId && pr.Paper != null)
         .Select(pr => pr.Paper!)
         .Include(p => p.Conference) 
         .Include(p => p.PaperPhase) 
         .ToListAsync();
        }

        public async Task<List<PaperReviewer>> GetPaperReviewersByPaperIdAsync(string paperId)
        {
            return await _context.Set<PaperReviewer>()
                .Include(pr => pr.User)
                .Include(pr => pr.Paper)
                    .ThenInclude(p => p.Conference)
                .Where(pr => pr.PaperId == paperId)
                .ToListAsync();
        }

        public async Task<List<PaperReviewer>> GetPaperReviewersByUserIdAsync(string userId)
        {
            return await _context.Set<PaperReviewer>()
                .Include(pr => pr.User)
                .Include(pr => pr.Paper)
                    .ThenInclude(p => p.Conference)
                .Where(pr => pr.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PaperReviewer>> GetHeadReviewersByPaperIdAsync(string paperId)
        {
            return await _context.Set<PaperReviewer>()
                .Where(pr => pr.PaperId == paperId && pr.IsHeadReviewer == true)
                .ToListAsync();
        }

        public async Task<List<PaperReviewer>> GetPaperReviewersByUserIdAndConferenceIdAsync(string userId, string conferenceId)
        {
            return await _context.Set<PaperReviewer>()
                .Include(pr => pr.Paper)
                    .ThenInclude(p => p.PaperPhase)
                .Where(pr => pr.UserId == userId && pr.Paper.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}