using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IGlobalStatusRepository
    {
        Task<GlobalStatus?> GetGlobalStatusByName(string globalStatusName);
        Task<int> CreateMutipleGlobalStatusesAsync(IEnumerable<GlobalStatus> globalStatuses);
        Task<int> CreateGlobalStatus(GlobalStatus globalStatus);
        Task<GlobalStatus> GetGlobalStatusByIdAsync(string globalStatusId);
        Task<int> UpdateGlobalStatusAsync(GlobalStatus globalStatus);
        Task<bool> DeleteGlobalStatusAsync(GlobalStatus globalStatus);
    }
    public class GlobalStatusRepository : GenericRepository<GlobalStatus>, IGlobalStatusRepository
    {
        public GlobalStatusRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<GlobalStatus?> GetGlobalStatusByName(string globalStatusName)
        {
            return await _context.GlobalStatuses.FirstOrDefaultAsync(x => x.Name == globalStatusName);
        }
        public async Task<int> CreateMutipleGlobalStatusesAsync(IEnumerable<GlobalStatus> globalStatuses)
        {
            await _context.GlobalStatuses.AddRangeAsync(globalStatuses);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> CreateGlobalStatus(GlobalStatus globalStatus)
        {
            return await CreateAsync(globalStatus);
        }
        public async Task<GlobalStatus> GetGlobalStatusByIdAsync(string globalStatusId)
        {
            return await GetByIdAsync(globalStatusId);
        }
        public async Task<int> UpdateGlobalStatusAsync(GlobalStatus globalStatus)
        {
            return await UpdateAsync(globalStatus);
        }
        public async Task<bool> DeleteGlobalStatusAsync(GlobalStatus globalStatus)
        {
            return await RemoveAsync(globalStatus);
        }
    }
}
