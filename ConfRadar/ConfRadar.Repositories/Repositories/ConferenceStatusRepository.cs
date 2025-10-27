using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceStatusRepository
    {
        Task<int> CreateConferenceStatusAsync(ConferenceStatus status);
        Task<int> CreateMultipleConferenceStatusAsync(List<ConferenceStatus> statuses);
        Task<int> UpdateConferenceStatusAsync(ConferenceStatus status);
        Task<int> DeleteConferenceStatusAsync(ConferenceStatus status);
        Task<ConferenceStatus?> GetConferenceStatusByIdAsync(string statusId);
        Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync();
        Task<ConferenceStatus?> GetConferenceStatusByNameAsync(string name);
    }
    public class ConferenceStatusRepository : GenericRepository<ConferenceStatus>, IConferenceStatusRepository
    {
        public ConferenceStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceStatusAsync(ConferenceStatus status)
        {
            return await CreateAsync(status);
        }

        public async Task<int> CreateMultipleConferenceStatusAsync(List<ConferenceStatus> statuses)
        {
            await _context.ConferenceStatuses.AddRangeAsync(statuses);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateConferenceStatusAsync(ConferenceStatus status)
        {
            return await UpdateAsync(status);
        }

        public async Task<int> DeleteConferenceStatusAsync(ConferenceStatus status)
        {
            _context.ConferenceStatuses.Remove(status);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceStatus?> GetConferenceStatusByIdAsync(string statusId)
        {
            return await _context.ConferenceStatuses
                .FirstOrDefaultAsync(s => s.ConferenceStatusId == statusId);
        }

        public async Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync()
        {
            return await _context.ConferenceStatuses.ToListAsync();
        }
        public async Task<ConferenceStatus?> GetConferenceStatusByNameAsync(string name)
        {
            return await _context.ConferenceStatuses
                .FirstOrDefaultAsync(s => s.ConferenceStatusName == name);
        }
    }

}
