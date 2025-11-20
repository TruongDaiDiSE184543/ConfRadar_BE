using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{


    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetMutipleUserRolesByUserId(string userId);
        Task<int> CreateUserRoleAsync(UserRole userRole);
        Task<UserRole?> GetUserRoleByUserAndRole(string userId, string roleId);
        Task<int> UpdateUserRole(UserRole userRole);
    }
    public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<int> CreateUserRoleAsync(UserRole userRole)
        {
            return await CreateAsync(userRole);
        }
        public async Task<IEnumerable<UserRole>> GetMutipleUserRolesByUserId(string userId)
        {
            return await _context.UserRoles.Include(x => x.Role).Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<UserRole?> GetUserRoleByUserAndRole(string userId, string roleId)
        {
            return await _context.UserRoles
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
        }
        public async Task<int> UpdateUserRole(UserRole userRole)
        {
            return await UpdateAsync(userRole);
        }
    }
}
