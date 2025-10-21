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
    }
}
