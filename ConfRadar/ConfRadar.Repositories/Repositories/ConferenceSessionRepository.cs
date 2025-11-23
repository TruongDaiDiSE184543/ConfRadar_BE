using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceSessionRepository
    {
        Task<int> CreateConferenceSessionAsync(ConferenceSession session);
        Task<int> CreateListConferenceSessionAsync(List<ConferenceSession> sessions);
        Task<int> UpdateConferenceSessionAsync(ConferenceSession session);
        Task<int> DeleteConferenceSessionAsync(ConferenceSession session);
        Task<ConferenceSession?> GetConferenceSessionByIdAsync(string sessionId);
        Task<List<ConferenceSession>> GetAllConferenceSessionsAsync();
        Task<List<ConferenceSession>> GetSessionsByConferenceIdAsync(string conferenceId);
        Task<List<ConferenceSession>> GetSessionsByConferenceIdWithRoomAsync(string conferenceId);
        Task<ConferenceSession?> GetSessionWithDetailsAsync(string sessionId);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateRangeAsync(string roomId, DateOnly startDate, DateOnly endDate);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateAsync(string roomId, DateOnly date);
        Task<List<ConferenceSession>> GetSessionsByRoomIdOverlappingTimeAsync(string roomId, DateOnly date, DateTime startTime, DateTime endTime);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAtTimeAsync(string roomId, DateOnly date, DateTime checkTime);
        Task<List<ConferenceSession>> GetSessionsByRoomIdOnDateAsync(string roomId, DateOnly date);
        Task<List<ConferenceSession>> GetSessionsByRoomIdsAndDateAsync(List<string> roomIds, DateOnly date);
        bool AnyTechSessionWithSpeaker(string techConfId);
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
            return await _context.ConferenceSessions.Include(cs => cs.Conference)
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

        public async Task<List<ConferenceSession>> GetSessionsByConferenceIdWithRoomAsync(string conferenceId)
        {
            return await _context.ConferenceSessions
                .Include(cs => cs.Room).ThenInclude(r => r.Destination).ThenInclude(d => d.City)
                .Where(cs => cs.ConferenceId == conferenceId)
                .ToListAsync();
        }

        public async Task<ConferenceSession?> GetSessionWithDetailsAsync(string sessionId)
        {
            return await _context.ConferenceSessions
                .Include(cs => cs.Conference)
                .Include(cs => cs.Room)
                    .ThenInclude(r => r.Destination)
                .Include(cs => cs.Speakers)
                .FirstOrDefaultAsync(cs => cs.ConferenceSessionId == sessionId);
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateRangeAsync(string roomId, DateOnly startDate, DateOnly endDate)
        {

            return await _context.ConferenceSessions.Include(s => s.Room).ThenInclude(r => r.Destination).ThenInclude(d => d.City)
                .Where(cs => cs.RoomId == roomId &&
                             cs.SessionDate >= startDate &&
                             cs.SessionDate <= endDate)
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateAsync(string roomId, DateOnly date)
        {
            // Get sessions on the specified date
            // For PostgreSQL timestamp without time zone, use DateTimeKind.Unspecified

            // Create start and end times for the date with Unspecified kind
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var endDateTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Unspecified);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.SessionDate == date) // Check if the StartTime is on the specified date
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdOverlappingTimeAsync(string roomId, DateOnly date, DateTime startTime, DateTime endTime)
        {
            // For PostgreSQL timestamp without time zone, use DateTimeKind.Unspecified
            // Convert the date part - start and end of the specified date

            // Convert the time parameters to DateTimeKind.Unspecified to match database format
            DateTime queryStartTime, queryEndTime;

            queryStartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            queryEndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.SessionDate == date &&
                            cs.StartTime < queryEndTime && // New session starts before existing ends
                            cs.EndTime > queryStartTime)   // New session ends after existing starts
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAtTimeAsync(string roomId, DateOnly date, DateTime checkTime)
        {
            // For PostgreSQL timestamp without time zone, use DateTimeKind.Unspecified

            // Convert the checkTime parameter to DateTimeKind.Unspecified to match database format
            var queryCheckTime = DateTime.SpecifyKind(checkTime, DateTimeKind.Unspecified);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.SessionDate == date &&
                            cs.StartTime <= queryCheckTime && // Session started before or at the check time
                            cs.EndTime > queryCheckTime)       // Session hasn't ended yet
                .ToListAsync();
        }

        //public async Task<List<ConferenceSession>> GetSessionsByRoomIdOnDateAsync(string roomId, DateOnly date)
        //{
        //    var dateStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        //    var dateEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);

        //    return await _context.ConferenceSessions
        //        .Where(cs => cs.RoomId == roomId && 
        //                    cs.Date >= dateStart && 
        //                    cs.Date < dateEnd)//.AddDays(1)) // From start of day to start of next day
        //        .ToListAsync();
        //}

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdOnDateAsync(string roomId, DateOnly date)
        {
            // Define the 24-hour window for the given local date. No tricky conversions.
            //var startOfDay = date.ToDateTime(TimeOnly.MinValue); // e.g., June 5, 00:00:00
            //var endOfDay = date.ToDateTime(TimeOnly.MaxValue);   // e.g., June 5, 23:59:59.99...

            // The query is now simple and easy to read. It finds all sessions
            // where the stored local start time falls within the local day.
            return await _context.ConferenceSessions.Include(s => s.Room).ThenInclude(r => r.Destination).ThenInclude(d => d.City)
                .Where(cs => cs.RoomId == roomId &&
                             cs.SessionDate.HasValue &&
                             cs.SessionDate == date
                             )
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdsAndDateAsync(List<string> roomIds, DateOnly date)
        {
            // Get sessions for multiple rooms on a specific date
            return await _context.ConferenceSessions
                .Where(cs => roomIds.Contains(cs.RoomId) &&
                             cs.SessionDate.HasValue &&
                             cs.SessionDate == date)
                .ToListAsync();
        }

        public async Task<int> CreateListConferenceSessionAsync(List<ConferenceSession> sessions)
        {
            await _context.ConferenceSessions.AddRangeAsync(sessions);
            return await _context.SaveChangesAsync();
        }

        public bool AnyTechSessionWithSpeaker(string techConfId)
        {
            return _context.ConferenceSessions.Include(cs => cs.Speakers).Any(cs => cs.ConferenceId == techConfId && cs.Speakers.Any());
        }
    }
}