using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IUserSuspendHistoryRepository
    {
        Task<int> CreateSuspensionAsync(UserSuspendHistory suspension);
        Task<int> UpdateSuspensionAsync(UserSuspendHistory suspension);
        Task<List<UserSuspendHistory>> GetUserSuspendHistoriesByUser(string userId);
        Task<UserSuspendHistory?> GetCurrentUserSuspendHistoryByUser(string userId);
    }

    public class UserSuspendHistoryRepository : GenericRepository<UserSuspendHistory>, IUserSuspendHistoryRepository
    {

        public UserSuspendHistoryRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateSuspensionAsync(UserSuspendHistory suspension)
        {
            return await CreateAsync(suspension);
        }

        public async Task<List<UserSuspendHistory>> GetUserSuspendHistoriesByUser(string userId)
        {
            return await _context.UserSuspendHistories.Where(ush => ush.UserId == userId).ToListAsync();
        }
        public async Task<UserSuspendHistory?> GetCurrentUserSuspendHistoryByUser(string userId)
        {
            return await _context.UserSuspendHistories.FirstOrDefaultAsync(ush => ush.UserId == userId && ush.IsActiveSuspend == true);
        }
        public async Task<int> UpdateSuspensionAsync(UserSuspendHistory suspension)
        {
            return await UpdateAsync(suspension);
        }
    }

}
