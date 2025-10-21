using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ISpeakerRepository
    {
        Task<int> CreateSpeakerAsync(Speaker speaker);
        Task<int> UpdateSpeakerAsync(Speaker speaker);
        Task<int> DeleteSpeakerAsync(Speaker speaker);
        Task<Speaker?> GetSpeakerByIdAsync(string sessionId);
        Task<List<Speaker>> GetAllSpeakersAsync();
    }

    public class SpeakerRepository : GenericRepository<Speaker>, ISpeakerRepository
    {
        public SpeakerRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateSpeakerAsync(Speaker speaker)
        {
            return await CreateAsync(speaker);
        }

        public async Task<int> UpdateSpeakerAsync(Speaker speaker)
        {
            return await UpdateAsync(speaker);
        }

        public async Task<int> DeleteSpeakerAsync(Speaker speaker)
        {
            _context.Speakers.Remove(speaker);
            return await _context.SaveChangesAsync();
        }

        public async Task<Speaker?> GetSpeakerByIdAsync(string sessionId)
        {
            return await _context.Speakers
                .FirstOrDefaultAsync(c => c.ConferenceSessionId == sessionId);
        }

        public async Task<List<Speaker>> GetAllSpeakersAsync()
        {
            return await _context.Speakers.ToListAsync();
        }
    }
}