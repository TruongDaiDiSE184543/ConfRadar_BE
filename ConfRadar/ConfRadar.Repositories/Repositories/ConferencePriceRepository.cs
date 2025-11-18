using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;


namespace ConfRadar.Repositories.Repositories
{
    public interface IConferencePriceRepository
    {
        //Task<ConferencePrice?> GetConferencePriceByConferencePriceId(string conferencePriceId);
        Task<int> CreateConferencePriceAsync(ConferencePrice price);
        Task<int> CreateMutipleConferencePriceAsync(List<ConferencePrice> price);
        Task<int> UpdateConferencePriceAsync(ConferencePrice price);
        Task<int> DeleteConferencePriceAsync(ConferencePrice price);
        Task<ConferencePrice?> GetConferencePriceByIdAsync(string priceId);
        Task<List<ConferencePrice>> GetAllConferencePricesAsync();
        Task<List<ConferencePrice>> GetPricesByConferenceIdAsync(string conferenceId);
        IQueryable<ConferencePrice> GetConferencePricesWithIncludes();
        Task<ConferencePrice?> GetConferencePriceWithIncludesAsync(string priceId);
        Task<List<ConferencePrice>> GetNumberOfIsAuthorByConferenceId(string confId);
        Task<ConferencePrice> AnyConferencePriceWithAtLeastOnePricePhase(string confId);
        Task<ConferencePrice> GetPricesWithDetailsByConferenceIdAsync (string confId);

    }

    public class ConferencePriceRepository : GenericRepository<ConferencePrice>, IConferencePriceRepository
    {
        public ConferencePriceRepository(ConfRadarDbContext context) : base(context) { }
        //public async Task<ConferencePrice?> GetConferencePriceByConferencePriceId(string conferencePriceId)
        //{
        //    return await _context.ConferencePrices
        //        .Include(x => x.PricePhases)
        //        .Include(x => x.Conference)
        //            .ThenInclude(x => x.ConferenceSessions)
        //        .AsSplitQuery()
        //        .FirstOrDefaultAsync(x => x.ConferencePriceId == conferencePriceId);
        //}

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
                .Include(cp => cp.PricePhases)
                .Include(c => c.Conference)
                    .ThenInclude(c => c.ResearchConferencePhases)
                .Include(c => c.Conference)
                    .ThenInclude(c => c.ConferenceSessions)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.ConferencePriceId == priceId);
        }

        public async Task<List<ConferencePrice>> GetAllConferencePricesAsync()
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhases)
                .ToListAsync();
        }

        public async Task<List<ConferencePrice>> GetPricesByConferenceIdAsync(string conferenceId)
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhases)
                .Where(cp => cp.ConferenceId == conferenceId)
                .ToListAsync();
        }

        public IQueryable<ConferencePrice> GetConferencePricesWithIncludes()
        {
            return _context.ConferencePrices
                .Include(cp => cp.PricePhases)
                .Include(cp => cp.Conference);
        }

        public async Task<ConferencePrice?> GetConferencePriceWithIncludesAsync(string priceId)
        {
            return await _context.ConferencePrices
                .Include(cp => cp.PricePhases)
                .Include(cp => cp.Conference)
                .FirstOrDefaultAsync(cp => cp.ConferencePriceId == priceId);
        }

        public async Task<int> CreateMutipleConferencePriceAsync(List<ConferencePrice> price)
        {
            await _context.ConferencePrices.AddRangeAsync(price);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<ConferencePrice>> GetNumberOfIsAuthorByConferenceId(string confId)
        {
            return await _context.ConferencePrices.Where(cp => cp.ConferenceId == confId && cp.IsAuthor == true).ToListAsync();
        }

        public async Task<ConferencePrice> AnyConferencePriceWithAtLeastOnePricePhase(string confId)
        {
            return await _context.ConferencePrices.Include(cp => cp.PricePhases).FirstOrDefaultAsync(cp => cp.ConferenceId == confId &&
            cp.PricePhases.Any());
        }

        public async Task<List<ConferencePrice>> GetPricesWithDetailsByConferenceIdAsync(string confId)
        {
            return await _context.ConferencePrices.Include(cp => cp.PricePhases).ThenInclude(pp => pp.RefundPolicies).Where(cp => cp.ConferenceId == confId).ToListAsync();
        }
    }
}

