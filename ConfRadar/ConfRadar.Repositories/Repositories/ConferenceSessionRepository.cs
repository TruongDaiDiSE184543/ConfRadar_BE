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
        Task<ConferenceSession?> GetSessionWithDetailsAsync(string sessionId);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateRangeAsync(string roomId, DateTime startDate, DateTime endDate);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateAsync(string roomId, DateOnly date);
        Task<List<ConferenceSession>> GetSessionsByRoomIdOverlappingTimeAsync(string roomId, DateOnly date, DateTime startTime, DateTime endTime);
        Task<List<ConferenceSession>> GetSessionsByRoomIdAtTimeAsync(string roomId, DateOnly date, DateTime checkTime);
        Task<List<ConferenceSession>> GetSessionsByRoomIdOnDateAsync(string roomId, DateOnly date);
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
                .Include(cs => cs.Speakers)
                .FirstOrDefaultAsync(cs => cs.ConferenceSessionId == sessionId);
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateRangeAsync(string roomId, DateTime startDate, DateTime endDate)
        {
            // Ensure DateTime parameters are in UTC
            var utcStartDate = startDate.Kind == DateTimeKind.Utc ? startDate : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEndDate = endDate.Kind == DateTimeKind.Utc ? endDate : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.StartTime >= utcStartDate &&
                            cs.EndTime <= utcEndDate)
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAndDateAsync(string roomId, DateOnly date)
        {
            // Get sessions on the specified date
            // Extract date from StartTime field since Date field has been removed
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.StartTime >= startDateTime &&
                            cs.StartTime <= endDateTime) // Check if the StartTime is on the specified date
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdOverlappingTimeAsync(string roomId, DateOnly date, DateTime startTime, DateTime endTime)
        {
            var dateStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var dateEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);

            // Ensure time parameters are in UTC
            var utcStartTime = startTime.Kind == DateTimeKind.Unspecified ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            var utcEndTime = endTime.Kind == DateTimeKind.Unspecified ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.StartTime >= dateStart &&
                            cs.StartTime <= dateEnd &&
                            cs.StartTime < utcEndTime && // New session starts before existing ends
                            cs.EndTime > utcStartTime)   // New session ends after existing starts
                .ToListAsync();
        }

        public async Task<List<ConferenceSession>> GetSessionsByRoomIdAtTimeAsync(string roomId, DateOnly date, DateTime checkTime)
        {
            var dateStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var dateEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);

            // Ensure checkTime parameter is in UTC
            var utcCheckTime = checkTime.Kind == DateTimeKind.Unspecified ? checkTime : DateTime.SpecifyKind(checkTime, DateTimeKind.Unspecified);

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                            cs.StartTime >= dateStart &&
                            cs.StartTime <= dateEnd &&
                            cs.StartTime <= utcCheckTime && // Session started before or at the check time
                            cs.EndTime > utcCheckTime)       // Session hasn't ended yet
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
            // These define the 24-hour window for the given date in UTC
            //var dateStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            //var dateEnd = dateStart.AddDays(1); // From the start of the day to the start of the next day

            return await _context.ConferenceSessions
                .Where(cs => cs.RoomId == roomId &&
                             cs.StartTime.HasValue && 
                             cs.EndTime.HasValue && cs.SessionDate == date)
                .ToListAsync();
        }

        public async Task<int> CreateListConferenceSessionAsync(List<ConferenceSession> sessions)
        {
            await _context.ConferenceSessions.AddRangeAsync(sessions);
            return await _context.SaveChangesAsync();
        }
    }
}