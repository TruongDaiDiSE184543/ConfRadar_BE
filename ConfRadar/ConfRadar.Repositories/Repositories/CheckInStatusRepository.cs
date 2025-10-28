using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICheckInStatusRepository
    {
        Task<int> CreateCheckInStatusAsync(CheckinStatus status);
        Task<int> UpdateCheckInStatusAsync(CheckinStatus status);
        Task<bool> DeleteCheckInStatusAsync(CheckinStatus status);
        Task<CheckinStatus?> GetCheckInStatusByIdAsync(string statusId);
        Task<CheckinStatus?> GetCheckInStatusByNameAsync(string statusName);
        Task<List<CheckinStatus>> GetAllCheckInStatusesAsync();
        Task<int> CreateMultipleCheckInStatusesAsync(List<CheckinStatus> statuses);
    }
    public class CheckInStatusRepository : GenericRepository<CheckinStatus>, ICheckInStatusRepository
    {
        public CheckInStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateCheckInStatusAsync(CheckinStatus status)
        {
            return await CreateAsync(status);
        }

        public async Task<int> UpdateCheckInStatusAsync(CheckinStatus status)
        {
            return await UpdateAsync(status);
        }

        public async Task<bool> DeleteCheckInStatusAsync(CheckinStatus status)
        {
            return await RemoveAsync(status);
        }

        public async Task<CheckinStatus?> GetCheckInStatusByIdAsync(string statusId)
        {
            return await GetByIdAsync(statusId);
        }

        public async Task<CheckinStatus?> GetCheckInStatusByNameAsync(string statusName)
        {
            return await _context.CheckinStatuses
                .FirstOrDefaultAsync(s => s.CheckinStatusName == statusName);
        }

        public async Task<List<CheckinStatus>> GetAllCheckInStatusesAsync()
        {
            return await GetAllAsync();
        }

        public async Task<int> CreateMultipleCheckInStatusesAsync(List<CheckinStatus> statuses)
        {
            await _context.CheckinStatuses.AddRangeAsync(statuses);
            return await _context.SaveChangesAsync();
        }
    }
}
