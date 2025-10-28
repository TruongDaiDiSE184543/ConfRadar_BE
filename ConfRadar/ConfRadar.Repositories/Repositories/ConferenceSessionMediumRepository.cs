using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceSessionMediumRepository
    {
        Task<int> CreateConferenceSessionMediumAsync(ConferenceSessionMedium media);
        Task<int> UpdateConferenceSessionMediumAsync(ConferenceSessionMedium media);
        Task<int> DeleteConferenceSessionMediumAsync(ConferenceSessionMedium media);
        Task<ConferenceSessionMedium?> GetConferenceSessionMediumByIdAsync(string mediaId);
        Task<List<ConferenceSessionMedium>> GetAllConferenceSessionMediaAsync();
        Task<List<ConferenceSessionMedium>> GetMediaBySessionIdAsync(string sessionId);
    }

    public class ConferenceSessionMediumRepository : GenericRepository<ConferenceSessionMedium>, IConferenceSessionMediumRepository
    {
        public ConferenceSessionMediumRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceSessionMediumAsync(ConferenceSessionMedium media)
        {
            return await CreateAsync(media);
        }

        public async Task<int> UpdateConferenceSessionMediumAsync(ConferenceSessionMedium media)
        {
            return await UpdateAsync(media);
        }

        public async Task<int> DeleteConferenceSessionMediumAsync(ConferenceSessionMedium media)
        {
            _context.ConferenceSessionMedia.Remove(media);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceSessionMedium?> GetConferenceSessionMediumByIdAsync(string mediaId)
        {
            return await _context.ConferenceSessionMedia
                .FirstOrDefaultAsync(c => c.ConferenceSessionMediaId == mediaId);
        }

        public async Task<List<ConferenceSessionMedium>> GetMediaBySessionIdAsync(string sessionId)
        {
            return await _context.ConferenceSessionMedia
                .Where(c => c.ConferenceSessionId == sessionId)
                .ToListAsync();
        }

        public async Task<List<ConferenceSessionMedium>> GetAllConferenceSessionMediaAsync()
        {
            return await _context.ConferenceSessionMedia.ToListAsync();
        }
    }
}