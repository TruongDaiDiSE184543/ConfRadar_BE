using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferencePriceRepository
    {
        Task<int> CreateConferencePriceAsync(ConferencePrice price);
        Task<int> UpdateConferencePriceAsync(ConferencePrice price);
        Task<int> DeleteConferencePriceAsync(ConferencePrice price);
        Task<ConferencePrice?> GetConferencePriceByIdAsync(string priceId);
        Task<List<ConferencePrice>> GetAllConferencePricesAsync();
        Task<List<ConferencePrice>> GetPricesByConferenceIdAsync(string conferenceId);
    }

    public class ConferencePriceRepository : GenericRepository<ConferencePrice>, IConferencePriceRepository
    {
        public ConferencePriceRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferencePriceAsync(ConferencePrice price)
        {
            return await CreateAsync(price);
        }

        public async Task<int> UpdateConferencePriceAsync(ConferencePrice price)
        {
            return await UpdateAsync(price);
        }

        public async Task<int> DeleteConferencePriceAsync(ConferencePrice price)
        {
            _context.ConferencePrices.Remove(price);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferencePrice?> GetConferencePriceByIdAsync(string priceId)
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhase)
                .FirstOrDefaultAsync(c => c.ConferencePriceId == priceId);
        }

        public async Task<List<ConferencePrice>> GetAllConferencePricesAsync()
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhase)
                .ToListAsync();
        }

        public async Task<List<ConferencePrice>> GetPricesByConferenceIdAsync(string conferenceId)
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhase)
                .Where(cp => cp.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}