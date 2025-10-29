using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperRepository
    {
        Task<int> CreatePaperAsync(Paper paper);
        Task<int> UpdatePaperAsync(Paper paper);
        Task<bool> DeletePaperAsync(Paper paper);
        Task<Paper?> GetPaperByIdAsync(string paperId);
        Task<Paper?> GetPaperByPaperIdAndUserIdAsync(string paperId, string userId);
        Task<Paper?> GetPaperByCameraReadyIdAsync(string cameraReadyId);
        Task<Paper?> GetPaperByFullPaperIdAsync(string fullPaperId);
        Task<List<Paper>> GetAllPapersAsync();
        Task<Paper?> GetPaperByUserAndConference(string conferenceId, string userId);
    }
    public class PaperRepository : GenericRepository<Paper>, IPaperRepository
    {
        public PaperRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreatePaperAsync(Paper paper)
        {
            return await CreateAsync(paper);
        }

        public async Task<int> UpdatePaperAsync(Paper paper)
        {
            return await UpdateAsync(paper);
        }

        public async Task<bool> DeletePaperAsync(Paper paper)
        {
            return await RemoveAsync(paper);
        }

        public async Task<Paper?> GetPaperByIdAsync(string paperId)
        {
            return await _context.Papers
                .Include(p => p.Conference)
                .ThenInclude(p => p.ResearchConferenceDetail)
                .FirstOrDefaultAsync(p => p.PaperId == paperId);
        }

        public async Task<List<Paper>> GetAllPapersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<Paper?> GetPaperByPaperIdAndUserIdAsync(string paperId, string userId)
        {
            return await _context.Papers.Include(p => p.Presenter).FirstOrDefaultAsync(p => p.PaperId == paperId && p.PresenterId == userId);
        }

        public async Task<Paper?> GetPaperByCameraReadyIdAsync(string cameraReadyId)
        {
            return await _context.Papers
                .Include(p => p.Presenter)
                .FirstOrDefaultAsync(p => p.CameraReadyId == cameraReadyId);
        }

        public async Task<Paper?> GetPaperByFullPaperIdAsync(string fullPaperId)
        {
            return await _context.Papers
                .Include(p => p.Presenter)
                .FirstOrDefaultAsync(p => p.FullPaperId == fullPaperId);
        }

        public async Task<Paper?> GetPaperByUserAndConference(string conferenceId, string userId)
        {
            return await _context.Papers
               .Include(p => p.Conference)
               .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.PresenterId == userId);
        }
    }
}
