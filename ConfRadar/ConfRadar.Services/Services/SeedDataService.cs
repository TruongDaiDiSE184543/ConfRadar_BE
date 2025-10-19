using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface ISeedDataService
    {
        Task SeedRolesAsync(IEnumerable<string> roles);
    }
    public class SeedDataService : ISeedDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SeedDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task SeedRolesAsync(IEnumerable<string> roles)
        {
            List<Role> roleList = new List<Role>();
            foreach (var role in roles)
            {
                var roleFound = await _unitOfWork.RoleRepository.GetRoleByRoleName(role);
                if (roleFound == null)
                {
                    roleFound = new Role()
                    {
                        RoleId = Guid.NewGuid().ToString(),
                        RoleName = role
                    };
                    roleList.Add(roleFound);
                }
               
            }
            if (roleList.Count > 0)
            {
                await _unitOfWork.RoleRepository.CreateMutipleRoleAsync(roleList);
            }
        }
    }
}
