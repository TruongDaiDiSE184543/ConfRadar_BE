using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IResearchConferencePhaseRepository
    {
        Task<int> CreateResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase);
        Task<int> UpdateResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase);
        Task<int> DeleteResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase);
        Task<ResearchConferencePhase?> GetResearchConferencePhaseByConferenceIdAsync(string conferenceId);
        Task<ResearchConferencePhase?> GetResearchConferencePhaseByIdAsync(string phaseId);
        Task<List<RevisionRoundDeadline>> GetRevisionRoundDeadlinesByPhaseIdAsync(string phaseId);
        Task<List<ResearchConferencePhase>> GetResearchPhaseByConfId(string confId);
    }

    public class ResearchConferencePhaseRepository
        : GenericRepository<ResearchConferencePhase>, IResearchConferencePhaseRepository
    {
        public ResearchConferencePhaseRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase)
        {
            return await CreateAsync(researchConferencePhase);
        }

        public async Task<int> UpdateResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase)
        {
            return await UpdateAsync(researchConferencePhase);
        }

        public async Task<int> DeleteResearchConferencePhaseAsync(ResearchConferencePhase researchConferencePhase)
        {
            _context.ResearchConferencePhases.Remove(researchConferencePhase);
            return await _context.SaveChangesAsync();
        }

        public async Task<ResearchConferencePhase?> GetResearchConferencePhaseByConferenceIdAsync(string conferenceId)
        {
            return await _context.ResearchConferencePhases
                .Include(r => r.RevisionRoundDeadlines) // Include related RevisionRoundDeadlines
                .FirstOrDefaultAsync(r => r.ConferenceId == conferenceId);
        }

        public async Task<ResearchConferencePhase?> GetResearchConferencePhaseByIdAsync(string phaseId)
        {
            return await _context.ResearchConferencePhases
                .FirstOrDefaultAsync(r => r.ResearchConferencePhaseId == phaseId);
        }

        public async Task<List<RevisionRoundDeadline>> GetRevisionRoundDeadlinesByPhaseIdAsync(string phaseId)
        {
            return await _context.RevisionRoundDeadlines
                .Where(r => r.ResearchConferencePhaseId == phaseId)
                .ToListAsync();
        }

        public async Task<List<ResearchConferencePhase>> GetResearchPhaseByConfId(string confId)
        {
            return await _context.ResearchConferencePhases.Include(rp => rp.Conference).Where(rp => rp.ConferenceId == confId).ToListAsync();
        }
    }
}