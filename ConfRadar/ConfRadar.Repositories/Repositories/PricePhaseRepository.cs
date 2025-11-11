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
        Task<PricePhase?> GetPricePhaseByConferencePriceIdAsync(string conferencePriceId);
        Task<List<PricePhase>> GetAllPricePhasesAsync();
        // Additional methods for CRUD operations on PricePhase
        Task<int> CreatePricePhasesForConferencePriceAsync(string conferencePriceId, List<PricePhase> pricePhases);
    }
    public class PricePhaseRepository : GenericRepository<PricePhase>, IPricePhaseRepository
    {
        public PricePhaseRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<PricePhase?> GetPricePhaseByPricePhaseId(string pricePhaseId)
        {

            return await _context.PricePhases
                .Include(pp => pp.ConferencePrice)
                  .ThenInclude(cp => cp.Conference)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.PricePhaseId == pricePhaseId);
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

        public async Task<PricePhase?> GetPricePhaseByConferencePriceIdAsync(string conferencePriceId)
        {
            return await _context.PricePhases
                            .Include(pp => pp.ConferencePrice)
                                .ThenInclude(cp => cp.Conference)
                            .FirstOrDefaultAsync(pp => pp.ConferencePriceId == conferencePriceId);
        }

        public async Task<int> CreatePricePhasesForConferencePriceAsync(string conferencePriceId, List<PricePhase> pricePhases)
        {
            foreach (var pricePhase in pricePhases)
            {
                pricePhase.ConferencePriceId = conferencePriceId;
            }
            await _context.PricePhases.AddRangeAsync(pricePhases);
            return await _context.SaveChangesAsync();
        }
    }
}

