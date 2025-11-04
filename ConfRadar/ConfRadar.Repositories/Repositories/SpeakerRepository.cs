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
        Task<Speaker?> GetSpeakerByIdAsync(string speakerId);
        Task<Speaker?> GetSpeakerBySessionIdAsync(string sessionId);
        Task<List<Speaker>> GetSpeakersBySessionIdAsync(string sessionId);
        Task<List<Speaker>> GetAllSpeakersAsync();
        // Additional methods for CRUD operations on Speaker
        Task<int> CreateSpeakersForConferenceSessionAsync(string conferenceSessionId, List<Speaker> speakers);
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

        public async Task<Speaker?> GetSpeakerByIdAsync(string speakerId)
        {
            //return await GetByIdAsync(speakerId);
            return await _context.Speakers.FirstOrDefaultAsync(x => x.SpeakerId == speakerId);

        }

        public async Task<Speaker?> GetSpeakerBySessionIdAsync(string sessionId)
        {
            return await _context.Speakers
                .FirstOrDefaultAsync(s => s.ConferenceSessionId == sessionId);
        }

        public async Task<List<Speaker>> GetSpeakersBySessionIdAsync(string sessionId)
        {
            return await _context.Speakers
                .Where(s => s.ConferenceSessionId == sessionId)
                .ToListAsync();
        }

        public async Task<List<Speaker>> GetAllSpeakersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<int> CreateSpeakersForConferenceSessionAsync(string conferenceSessionId, List<Speaker> speakers)
        {
            foreach (var speaker in speakers)
            {
                speaker.ConferenceSessionId = conferenceSessionId;
            }
            await _context.Speakers.AddRangeAsync(speakers);
            return await _context.SaveChangesAsync();
        }
    }
}