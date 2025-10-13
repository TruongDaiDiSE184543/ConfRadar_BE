using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRoleRepository
    {
    }
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ConfRadarDbContext context) : base(context)
        {
        }
    }
}
