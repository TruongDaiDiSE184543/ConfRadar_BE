using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperPhaseRepository
    {
        Task<PaperPhase?> GetPaperPhaseByName(string phaseName);
        Task<int> CreateMultiplePaperPhasesAsync(IEnumerable<PaperPhase> paperPhases);
        Task<int> CreatePaperPhase(PaperPhase paperPhase);
        Task<PaperPhase> GetPaperPhaseByIdAsync(string paperPhaseId);
        Task<int> UpdatePaperPhaseAsync(PaperPhase paperPhase);
        Task<bool> DeletePaperPhaseAsync(PaperPhase paperPhase);
        Task<List<PaperPhase>> GetAllPaperPhaseAsync();
    }

    public class PaperPhaseRepository : GenericRepository<PaperPhase>, IPaperPhaseRepository
    {
        public PaperPhaseRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<PaperPhase?> GetPaperPhaseByName(string phaseName)
        {
            return await _context.PaperPhases.FirstOrDefaultAsync(x => x.PhaseName == phaseName);
        }

        public async Task<int> CreateMultiplePaperPhasesAsync(IEnumerable<PaperPhase> paperPhases)
        {
            await _context.PaperPhases.AddRangeAsync(paperPhases);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreatePaperPhase(PaperPhase paperPhase)
        {
            return await CreateAsync(paperPhase);
        }

        public async Task<PaperPhase> GetPaperPhaseByIdAsync(string paperPhaseId)
        {
            return await GetByIdAsync(paperPhaseId);
        }

        public async Task<int> UpdatePaperPhaseAsync(PaperPhase paperPhase)
        {
            return await UpdateAsync(paperPhase);
        }

        public async Task<bool> DeletePaperPhaseAsync(PaperPhase paperPhase)
        {
            return await RemoveAsync(paperPhase);
        }

        public async Task<List<PaperPhase>> GetAllPaperPhaseAsync()
        {
            return await GetAllAsync();
        }
    }
}