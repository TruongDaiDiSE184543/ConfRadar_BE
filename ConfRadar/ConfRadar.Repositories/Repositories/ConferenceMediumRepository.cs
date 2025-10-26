using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceMediumRepository
    {
        Task<int> CreateConferenceMediumAsync(ConferenceMedium media);
        Task<int> UpdateConferenceMediumAsync(ConferenceMedium media);
        Task<int> DeleteConferenceMediumAsync(ConferenceMedium media);
        Task<ConferenceMedium?> GetConferenceMediumByIdAsync(string mediaId);
        Task<List<ConferenceMedium>> GetAllConferenceMediaAsync();
        Task<List<ConferenceMedium>> GetMediaByConferenceIdAsync(string conferenceId);
    }

    public class ConferenceMediumRepository : GenericRepository<ConferenceMedium>, IConferenceMediumRepository
    {
        public ConferenceMediumRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceMediumAsync(ConferenceMedium media)
        {
            return await CreateAsync(media);
        }

        public async Task<int> UpdateConferenceMediumAsync(ConferenceMedium media)
        {
            return await UpdateAsync(media);
        }

        public async Task<int> DeleteConferenceMediumAsync(ConferenceMedium media)
        {
            _context.ConferenceMedia.Remove(media);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceMedium?> GetConferenceMediumByIdAsync(string mediaId)
        {
            return await _context.ConferenceMedia
                .FirstOrDefaultAsync(c => c.ConferenceMediaId == mediaId);
        }

        public async Task<List<ConferenceMedium>> GetAllConferenceMediaAsync()
        {
            return await _context.ConferenceMedia
                .ToListAsync();
        }

        public async Task<List<ConferenceMedium>> GetMediaByConferenceIdAsync(string conferenceId)
        {
            return await _context.ConferenceMedia
                .Where(cm => cm.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}