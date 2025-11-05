using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserCheckInRepository
    {
        Task<int> CreateUserCheckInAsync(UserCheckIn checkIn);
        Task<int> UpdateUserCheckInAsync(UserCheckIn checkIn);
        Task<bool> DeleteUserCheckInAsync(UserCheckIn checkIn);
        Task<UserCheckIn?> GetUserCheckInByUserIdAndConferenceSessionIdAsync(string sessionId, string userId);
    }
    public class UserCheckInRepository : GenericRepository<UserCheckIn>, IUserCheckInRepository
    {
        public UserCheckInRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateUserCheckInAsync(UserCheckIn checkIn)
        {
            return await CreateAsync(checkIn);
        }

        public async Task<int> UpdateUserCheckInAsync(UserCheckIn checkIn)
        {
            return await UpdateAsync(checkIn);
        }

        public async Task<bool> DeleteUserCheckInAsync(UserCheckIn checkIn)
        {
            return await RemoveAsync(checkIn);
        }

        public async Task<UserCheckIn?> GetUserCheckInByUserIdAndConferenceSessionIdAsync(string sessionId, string userId)
        {
            return await _context.UserCheckIns.FirstOrDefaultAsync(uci => uci.ConferenceSessionId == sessionId && uci.UserId == userId);
        }
    }
}
