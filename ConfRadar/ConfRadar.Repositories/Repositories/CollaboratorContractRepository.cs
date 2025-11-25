using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICollaboratorContractRepository
    {
        Task<int> CreateCollaboratorContractAsync(CollaboratorContract collaboratorContract);
        Task<int> UpdateCollaboratorContractAsync(CollaboratorContract collaboratorContract);
        Task<CollaboratorContract?> GetCollaboratorContractByIdAsync(string collaboratorContractId);
        Task<List<CollaboratorContract>> GetListCollaboratorContractByUserIdAsync(string userId);
    }
    public class CollaboratorContractRepository : GenericRepository<CollaboratorContract>, ICollaboratorContractRepository
    {
        public CollaboratorContractRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateCollaboratorContractAsync(CollaboratorContract collaboratorContract)
        {
            return await CreateAsync(collaboratorContract);
        }

        public async Task<CollaboratorContract?> GetCollaboratorContractByIdAsync(string collaboratorContractId)
        {
            return await _context.CollaboratorContracts
                .FirstOrDefaultAsync(cc => cc.CollaboratorContractId == collaboratorContractId);
        }
        public async Task<List<CollaboratorContract>> GetListCollaboratorContractByUserIdAsync(string userId)
        {
            return await _context.CollaboratorContracts
                .Include(cc => cc.User)
                .Include(cc => cc.Conference)
                .Where(cc => cc.UserId == userId).ToListAsync();
        }

        public async Task<int> UpdateCollaboratorContractAsync(CollaboratorContract collaboratorContract)
        {
            return await UpdateAsync(collaboratorContract);
        }
    }
}
