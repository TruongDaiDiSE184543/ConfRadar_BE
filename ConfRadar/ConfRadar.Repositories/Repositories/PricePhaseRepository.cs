using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

using Microsoft.EntityFrameworkCore;


namespace ConfRadar.Repositories.Repositories
{
    public interface IPricePhaseRepository
    {

        Task<PricePhase?> GetPricePhaseByPricePhaseId(string pricePhaseId);
        Task<int> CreatePricePhaseAsync(PricePhase pricePhase);
        Task<int> UpdatePricePhaseAsync(PricePhase pricePhase);
        Task<int> DeletePricePhaseAsync(PricePhase pricePhase);
        Task<PricePhase?> GetPricePhaseByIdAsync(string pricePhaseId);
        Task<List<PricePhase>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId);
        Task<List<PricePhase>> GetAllPricePhasesAsync();
    }
    public class PricePhaseRepository : GenericRepository<PricePhase>, IPricePhaseRepository
    {
        public PricePhaseRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<PricePhase?> GetPricePhaseByPricePhaseId(string pricePhaseId)
        {
            return await GetByIdAsync(pricePhaseId);
        }
        public async Task<int> CreatePricePhaseAsync(PricePhase pricePhase)
        {
            return await CreateAsync(pricePhase);
        }

        public async Task<int> UpdatePricePhaseAsync(PricePhase pricePhase)
        {
            return await UpdateAsync(pricePhase);
        }

        public async Task<int> DeletePricePhaseAsync(PricePhase pricePhase)
        {
            _context.PricePhases.Remove(pricePhase);
            return await _context.SaveChangesAsync();
        }

        public async Task<PricePhase?> GetPricePhaseByIdAsync(string pricePhaseId)
        {
            return await _context.PricePhases
                .FirstOrDefaultAsync(c => c.PricePhaseId == pricePhaseId);
        }

        public async Task<List<PricePhase>> GetPricePhasesByConferencePriceIdAsync(string conferencePriceId)
        {
            return await _context.PricePhases
                .Where(pp => pp.ConferencePriceId == conferencePriceId)
                .ToListAsync();
        }

        public async Task<List<PricePhase>> GetAllPricePhasesAsync()
        {
            return await _context.PricePhases.ToListAsync();
        }
    }
}

