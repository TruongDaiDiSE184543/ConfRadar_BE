using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{

    public interface IWaitListStatusRepository
    {
        Task<int> CreateWaitListStatusAsync(WaitListStatus status);
        Task<int> UpdateWaitListStatusAsync(WaitListStatus status);
        Task<bool> DeleteWaitListStatusAsync(WaitListStatus status);
        Task<WaitListStatus?> GetWaitListStatusByIdAsync(string statusId);
        Task<WaitListStatus?> GetWaitListStatusByNameAsync(string statusName);
        Task<List<WaitListStatus>> GetAllWaitListStatusesAsync();
        Task<int> CreateMultipleWaitListStatusesAsync(List<WaitListStatus> statuses);
    }
    public class WaitListStatusRepository : GenericRepository<WaitListStatus>, IWaitListStatusRepository
    {
        public WaitListStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateWaitListStatusAsync(WaitListStatus status)
        {
            return await CreateAsync(status);
        }

        public async Task<int> UpdateWaitListStatusAsync(WaitListStatus status)
        {
            return await UpdateAsync(status);
        }

        public async Task<bool> DeleteWaitListStatusAsync(WaitListStatus status)
        {
            return await RemoveAsync(status);
        }

        public async Task<WaitListStatus?> GetWaitListStatusByIdAsync(string statusId)
        {
            return await _context.WaitListStatuses.FirstOrDefaultAsync(x => x.WaitListStatusId == statusId);
        }

        public async Task<WaitListStatus?> GetWaitListStatusByNameAsync(string statusName)
        {
            return await _context.WaitListStatuses
                .FirstOrDefaultAsync(s => s.Name == statusName);
        }

        public async Task<List<WaitListStatus>> GetAllWaitListStatusesAsync()
        {
            return await GetAllAsync();
        }

        public async Task<int> CreateMultipleWaitListStatusesAsync(List<WaitListStatus> statuses)
        {
            await _context.WaitListStatuses.AddRangeAsync(statuses);
            return await _context.SaveChangesAsync();
        }
    }

}
