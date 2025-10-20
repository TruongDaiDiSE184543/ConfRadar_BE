using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceRepository
    {
        Task<int> CreateConferenceAsync(Conference conference);
        Task<int> UpdateConferenceAsync(Conference conference);
        Task<int> DeleteConferenceAsync(Conference conference);
        Task<Conference?> GetConferenceByIdAsync(string conferenceId);
        Task<List<Conference>> GetAllConferencesAsync();
        Task<Conference?> GetConferenceWithDetailsAsync(string conferenceId);
    }

    public class ConferenceRepository : GenericRepository<Conference>, IConferenceRepository
    {
        public ConferenceRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceAsync(Conference conference)
        {
            return await CreateAsync(conference);
        }

        public async Task<int> UpdateConferenceAsync(Conference conference)
        {
            return await UpdateAsync(conference);
        }

        public async Task<int> DeleteConferenceAsync(Conference conference)
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<Conference?> GetConferenceByIdAsync(string conferenceId)
        {
            return await _context.Conferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
        }

        public async Task<List<Conference>> GetAllConferencesAsync()
        {
            return await _context.Conferences.ToListAsync();
        }

        public async Task<Conference?> GetConferenceWithDetailsAsync(string conferenceId)
        {
            return await _context.Conferences
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                    .ThenInclude(cm => cm.MediaType)
                .Include(c => c.ConferencePolicies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhase)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room)
                        .ThenInclude(r => r.Destination)
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                .Include(c => c.FavouriteConferences)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
        }
    }
}