using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserCheckInRepository
    {
        Task<int> CreateUserCheckInAsync(UserCheckIn userCheckIn);
        Task<int> UpdateUserCheckInAsync(UserCheckIn userCheckIn);
        Task<bool> DeleteUserCheckInAsync(UserCheckIn userCheckIn);
        Task<UserCheckIn?> GetUserCheckInByIdAsync(string userCheckInId);
        Task<List<UserCheckIn>> GetAllUserCheckInsAsync();
        Task<UserCheckIn?> GetUserCheckInByUserAndSessionAsync(string userId, string sessionId);
        Task<List<UserCheckIn>> GetUserCheckinByPhaseId(string phaseId);
        Task<UserCheckIn> GetPresenterByTicket(string ticketId);
        Task<List<UserCheckIn>> GetUserCheckinsByTicketIdsAsync(List<string> allTicketIds);
    }

    public class UserCheckInRepository : GenericRepository<UserCheckIn>, IUserCheckInRepository
    {
        private readonly ConfRadarDbContext _context;

        public UserCheckInRepository(ConfRadarDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CreateUserCheckInAsync(UserCheckIn userCheckIn)
        {
            _context.UserCheckIns.Add(userCheckIn);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateUserCheckInAsync(UserCheckIn userCheckIn)
        {
            var tracker = _context.Attach(userCheckIn);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserCheckInAsync(UserCheckIn userCheckIn)
        {
            _context.UserCheckIns.Remove(userCheckIn);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserCheckIn?> GetUserCheckInByIdAsync(string userCheckInId)
        {
            return await _context.UserCheckIns
                .Include(uci => uci.CheckinStatus)
                .Include(uci => uci.ConferenceSession)
                .Include(uci => uci.Ticket)
                .Include(uci => uci.User)
                .FirstOrDefaultAsync(uci => uci.UserCheckinId == userCheckInId);
        }

        public async Task<List<UserCheckIn>> GetAllUserCheckInsAsync()
        {
            return await _context.UserCheckIns.ToListAsync();
        }

        public async Task<UserCheckIn?> GetUserCheckInByUserAndSessionAsync(string userId, string sessionId)
        {
            return await _context.UserCheckIns
                .FirstOrDefaultAsync(uci => uci.UserId == userId && uci.ConferenceSessionId == sessionId);
        }

        public async Task<UserCheckIn> GetPresenterByTicket(string ticketId)
        {
            return await _context.UserCheckIns.FirstOrDefaultAsync(usc => usc.TicketId == ticketId && usc.IsPresenter == true);
        }

        public async Task<List<UserCheckIn>> GetUserCheckinByPhaseId(string phaseId)
        {
            return await _context.UserCheckIns
                .Include(uc => uc.Ticket)
                    .ThenInclude(t => t.PricePhase)
                .Include(uc => uc.CheckinStatus)
                .Where(uc => uc.Ticket.PricePhaseId == phaseId).ToListAsync();
        }

        public async Task<List<UserCheckIn>> GetUserCheckinsByTicketIdsAsync(List<string> allTicketIds)
        {
            return await _context.UserCheckIns.Where(uc => uc.TicketId != null && allTicketIds.Contains(uc.TicketId)).ToListAsync();
        }
    }
}
