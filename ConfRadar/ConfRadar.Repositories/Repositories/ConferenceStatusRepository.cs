using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceStatusRepository
    {
        Task<ConferenceStatus?> GetConferenceStatusByName(string conferenceStatusName);
        Task<int> CreateMultipleConferenceStatusesAsync(IEnumerable<ConferenceStatus> conferenceStatuses);
        Task<int> CreateConferenceStatus(ConferenceStatus conferenceStatus);
        Task<ConferenceStatus> GetConferenceStatusByIdAsync(string conferenceStatusId);
        Task<int> UpdateConferenceStatusAsync(ConferenceStatus conferenceStatus);
        Task<bool> DeleteConferenceStatusAsync(ConferenceStatus conferenceStatus);
        Task<List<ConferenceStatus>> GetAllConferenceStatusAsync();
    }

    public class ConferenceStatusRepository : GenericRepository<ConferenceStatus>, IConferenceStatusRepository
    {
        public ConferenceStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<ConferenceStatus?> GetConferenceStatusByName(string conferenceStatusName)
        {
            return await _context.ConferenceStatuses.FirstOrDefaultAsync(x => x.ConferenceStatusName == conferenceStatusName);
        }

        public async Task<int> CreateMultipleConferenceStatusesAsync(IEnumerable<ConferenceStatus> conferenceStatuses)
        {
            await _context.ConferenceStatuses.AddRangeAsync(conferenceStatuses);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateConferenceStatus(ConferenceStatus conferenceStatus)
        {
            return await CreateAsync(conferenceStatus);
        }

        public async Task<ConferenceStatus> GetConferenceStatusByIdAsync(string conferenceStatusId)
        {
            return await GetByIdAsync(conferenceStatusId);
        }

        public async Task<int> UpdateConferenceStatusAsync(ConferenceStatus conferenceStatus)
        {
            return await UpdateAsync(conferenceStatus);
        }

        public async Task<bool> DeleteConferenceStatusAsync(ConferenceStatus conferenceStatus)
        {
            return await RemoveAsync(conferenceStatus);
        }

        public async Task<List<ConferenceStatus>> GetAllConferenceStatusAsync()
        {
            return await GetAllAsync();
        }
    }
}