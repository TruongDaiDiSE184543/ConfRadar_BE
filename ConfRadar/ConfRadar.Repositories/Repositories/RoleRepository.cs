using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRoleRepository
    {
        Task<int> CreateRoleAsync(Role role);
        Task<Role?> GetRoleByRoleName(string roleName);
        Task<int> CreateMutipleRoleAsync(IEnumerable<Role> roles);
        Task<List<Role>?> GetListRoleByListRoleName(List<string> roleNameList);

    }
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<int> CreateRoleAsync(Role role)
        {
            return await CreateAsync(role);
        }
        public async Task<int> CreateMutipleRoleAsync(IEnumerable<Role> roles)
        {
            await _context.Roles.AddRangeAsync(roles);
            return await _context.SaveChangesAsync();
        }
        public async Task<Role?> GetRoleByRoleName(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.RoleName == roleName);
        }
        public async Task<List<Role>?> GetListRoleByListRoleName(List<string> roleNameList)
        {
            return await _context.Roles.Where(r => roleNameList.Contains(r.RoleName)).ToListAsync();
        }

    }
}
