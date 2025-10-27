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
    public interface IPaperPhaseRepository
    {
        Task<int> CreatePaperPhaseAsync(PaperPhase paperPhase);
        Task<int> UpdatePaperPhaseAsync(PaperPhase paperPhase);
        Task<int> DeletePaperPhaseAsync(PaperPhase paperPhase);
        Task<PaperPhase?> GetPaperPhaseByIdAsync(string paperPhaseId);
        Task<List<PaperPhase>> GetAllPaperPhasesAsync();
        Task<int> CreateMultiplePaperPhasesAsync(List<PaperPhase> paperPhases);
        Task<PaperPhase?> GetPaperPhaseByNameAsync(string phaseName);
    }
    public class PaperPhaseRepository : GenericRepository<PaperPhase>, IPaperPhaseRepository
    {
        public PaperPhaseRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreatePaperPhaseAsync(PaperPhase paperPhase)
        {
            return await CreateAsync(paperPhase);
        }
        public async Task<PaperPhase?> GetPaperPhaseByNameAsync(string phaseName)
        {
            return await _context.PaperPhases
                .FirstOrDefaultAsync(p => p.PhaseName == phaseName);
        }
        public async Task<int> UpdatePaperPhaseAsync(PaperPhase paperPhase)
        {
            return await UpdateAsync(paperPhase);
        }

        public async Task<int> DeletePaperPhaseAsync(PaperPhase paperPhase)
        {
            _context.PaperPhases.Remove(paperPhase);
            return await _context.SaveChangesAsync();
        }

        public async Task<PaperPhase?> GetPaperPhaseByIdAsync(string paperPhaseId)
        {
            return await _context.PaperPhases
                .FirstOrDefaultAsync(p => p.PaperPhaseId == paperPhaseId);
        }

        public async Task<List<PaperPhase>> GetAllPaperPhasesAsync()
        {
            return await _context.PaperPhases.ToListAsync();
        }

        public async Task<int> CreateMultiplePaperPhasesAsync(List<PaperPhase> paperPhases)
        {
            await _context.PaperPhases.AddRangeAsync(paperPhases);
            return await _context.SaveChangesAsync();
        }
    }
}
