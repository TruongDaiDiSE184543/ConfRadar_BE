using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IOrganizationRepository
    {
        Task<int> CreateOrganizationAsync(Organization organization);
        Task<int> UpdateOrganizationAsync(Organization organization);
        Task<Organization?> GetOrganizationByIdAsync(string organizationId);
        Task<List<Organization>> GetAllOrganizationsAsync();
    }
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(ConfRadarDbContext context) : base(context)
        {

        }

        public async Task<int> CreateOrganizationAsync(Organization organization)
        {
            return await CreateAsync(organization);
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationByIdAsync(string organizationId)
        {
            return await _context.Organizations
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
        }

        public async Task<int> UpdateOrganizationAsync(Organization organization)
        {
            return await UpdateAsync(organization);
        }
    }
}
