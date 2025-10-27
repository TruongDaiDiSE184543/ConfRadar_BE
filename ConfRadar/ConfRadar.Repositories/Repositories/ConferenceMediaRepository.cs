using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceMediaRepository
    {
        Task<int> CreateConferenceMediaAsync(ConferenceMedium media);
        Task<int> CreateMutipleConferenceMediaAsync(List<ConferenceMedium> media);
        Task<int> UpdateConferenceMediaAsync(ConferenceMedium media);
        Task<int> DeleteConferenceMediaAsync(ConferenceMedium media);
        Task<ConferenceMedium?> GetConferenceMediaByIdAsync(string mediaId);
        Task<List<ConferenceMedium>> GetAllConferenceMediaAsync();
        Task<List<ConferenceMedium>> GetMediaByConferenceIdAsync(string conferenceId);
    }

    public class ConferenceMediaRepository : GenericRepository<ConferenceMedium>, IConferenceMediaRepository
    {
        public ConferenceMediaRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceMediaAsync(ConferenceMedium media)
        {
            return await CreateAsync(media);
        }

        public async Task<int> UpdateConferenceMediaAsync(ConferenceMedium media)
        {
            return await UpdateAsync(media);
        }

        public async Task<int> DeleteConferenceMediaAsync(ConferenceMedium media)
        {
            _context.ConferenceMedia.Remove(media);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceMedium?> GetConferenceMediaByIdAsync(string mediaId)
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

        public async Task<int> CreateMutipleConferenceMediaAsync(List<ConferenceMedium> media)
        {
            await _context.ConferenceMedia.AddRangeAsync(media);
            return await _context.SaveChangesAsync();   
        }
    }
}