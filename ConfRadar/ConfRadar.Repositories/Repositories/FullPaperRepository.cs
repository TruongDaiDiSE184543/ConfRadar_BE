using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IFullPaperRepository
    {
        Task<int> CreateFullPaperAsync(FullPaper fullPaper);
        Task<int> UpdateFullPaperAsync(FullPaper fullPaper);
        Task<bool> DeleteFullPaperAsync(FullPaper fullPaper);
        Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId);
        Task<List<FullPaper>> GetAllFullPapersAsync();
        Task<List<FullPaper>> GetFullPaperByStatusName(string status);
        Task<List<FullPaper>> GetExpiredFullPaper(DateOnly dateNow, ReviewStatus status, List<ConferenceStatus> confStatuses);
        Task<int> UpdateMutipleFullPaperAsync(List<FullPaper> fullPapers);
    }
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
        public async Task<int> UpdateMutipleFullPaperAsync(List<FullPaper> fullPapers)
        {
             _context.FullPapers.UpdateRange(fullPapers);
            return await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteFullPaperAsync(FullPaper fullPaper)
        {
            return await RemoveAsync(fullPaper);
        }

        public async Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId)
        {
            return await _context.FullPapers
                .Include(fp => fp.ReviewStatus)
                .FirstOrDefaultAsync(fp => fp.FullPaperId == fullPaperId);
        }

        public async Task<List<FullPaper>> GetAllFullPapersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<FullPaper>> GetFullPaperByStatusName(string status)
        {
            return await _context.FullPapers.Where(fp => fp.ReviewStatus.Name == status).ToListAsync();
        }

        public async Task<List<FullPaper>> GetExpiredFullPaper(DateOnly dateNow, ReviewStatus status, List<ConferenceStatus> confStatuses)
        {
            var confStatusIds = confStatuses
           .Select(c => c.ConferenceStatusId)
           .ToList();
            return await _context.FullPapers
                .Where(fp => fp.ReviewStatusId == status.ReviewStatusId
                && fp.Papers.Any(p => p.ResearchConferencePhase != null
                && p.ResearchConferencePhase.FullPaperEndDate < dateNow
                && p.Conference != null 
                && p.Conference.ConferenceStatus!=null
                && confStatusIds.Contains(p.Conference.ConferenceStatusId)))
                .ToListAsync();
        }
    }
}