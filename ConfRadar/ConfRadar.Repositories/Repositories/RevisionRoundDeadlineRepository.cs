using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRevisionRoundDeadlineRepository
    {
        Task<int> CreateCsAsync(RevisionRoundDeadline revisionRoundDeadline);
        Task<int> UpdateCsAsync(RevisionRoundDeadline revisionRoundDeadline);
        Task<int> DeleteCsAsync(RevisionRoundDeadline revisionRoundDeadline);
        Task<RevisionRoundDeadline?> GetCsByIdAsync(string revisionRoundDeadlineId);
        Task<List<RevisionRoundDeadline>> GetCsByPhaseIdAsync(string phaseId);
    }

    public class RevisionRoundDeadlineRepository : GenericRepository<RevisionRoundDeadline>, IRevisionRoundDeadlineRepository
    {
        public RevisionRoundDeadlineRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateCsAsync(RevisionRoundDeadline revisionRoundDeadline)
        {
            return await base.CreateAsync(revisionRoundDeadline);
        }

        public async Task<int> UpdateCsAsync(RevisionRoundDeadline revisionRoundDeadline)
        {
            return await base.UpdateAsync(revisionRoundDeadline);
        }

        public async Task<int> DeleteCsAsync(RevisionRoundDeadline revisionRoundDeadline)
        {
            _context.Remove(revisionRoundDeadline);
            return await _context.SaveChangesAsync();
        }

        public async Task<RevisionRoundDeadline?> GetCsByIdAsync(string revisionRoundDeadlineId)
        {
            return await _context.RevisionRoundDeadlines
                .FirstOrDefaultAsync(r => r.RevisionRoundDeadlineId == revisionRoundDeadlineId);
        }

        public async Task<List<RevisionRoundDeadline>> GetCsByPhaseIdAsync(string phaseId)
        {
            return await _context.RevisionRoundDeadlines
                .Where(r => r.ResearchConferencePhaseId == phaseId)
                .ToListAsync();
        }
    }
}