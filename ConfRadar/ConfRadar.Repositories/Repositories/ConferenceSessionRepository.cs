using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceSessionRepository
    {
        Task<int> CreateConferenceSessionAsync(ConferenceSession session);
        Task<int> UpdateConferenceSessionAsync(ConferenceSession session);
        Task<int> DeleteConferenceSessionAsync(ConferenceSession session);
        Task<ConferenceSession?> GetConferenceSessionByIdAsync(string sessionId);
        Task<List<ConferenceSession>> GetAllConferenceSessionsAsync();
        Task<List<ConferenceSession>> GetSessionsByConferenceIdAsync(string conferenceId);
        Task<ConferenceSession?> GetSessionWithDetailsAsync(string sessionId);
    }

    public class ConferenceSessionRepository : GenericRepository<ConferenceSession>, IConferenceSessionRepository
    {
        public ConferenceSessionRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceSessionAsync(ConferenceSession session)
        {
            return await CreateAsync(session);
        }

        public async Task<int> UpdateConferenceSessionAsync(ConferenceSession session)
        {
            return await UpdateAsync(session);
        }

        public async Task<int> DeleteConferenceSessionAsync(ConferenceSession session)
        {
            _context.ConferenceSessions.Remove(session);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceSession?> GetConferenceSessionByIdAsync(string sessionId)
        {
            return await _context.ConferenceSessions
                .FirstOrDefaultAsync(c => c.ConferenceSessionId == sessionId);
        }

        public async Task<List<ConferenceSession>> GetAllConferenceSessionsAsync()
        {
            return await _context.ConferenceSessions.ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByConferenceIdAsync(string conferenceId)
        {
            return await _context.ConferenceSessions
                .Where(cs => cs.ConferenceId == conferenceId)
                .ToListAsync();
        }

        public async Task<ConferenceSession?> GetSessionWithDetailsAsync(string sessionId)
        {
            return await _context.ConferenceSessions
                .Include(cs => cs.Conference)
                .Include(cs => cs.Room)
                    .ThenInclude(r => r.Destination)
                .Include(cs => cs.Speaker)
                .FirstOrDefaultAsync(cs => cs.ConferenceSessionId == sessionId);
        }
    }
}