using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPresenterChangeRequestRepository
    {
        Task<int> CreatePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest);
        Task<int> UpdatePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest);
        Task<bool> DeletePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest);
        Task<PresenterChangeRequest?> GetPresenterChangeRequestByIdAsync(string presenterChangeRequestId);
        Task<List<PresenterChangeRequest>> GetAllPresenterChangeRequestsAsync();
        Task<List<PresenterChangeRequest>> GetAllPresenterChangeRequestsByConfIdAndStatusIdAsync(string statusId, string confId);

    }

    public class PresenterChangeRequestRepository : GenericRepository<PresenterChangeRequest>, IPresenterChangeRequestRepository
    {
        private readonly ConfRadarDbContext _context;

        public PresenterChangeRequestRepository(ConfRadarDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CreatePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest)
        {
            _context.PresenterChangeRequests.Add(presenterChangeRequest);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest)
        {
            var tracker = _context.Attach(presenterChangeRequest);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletePresenterChangeRequestAsync(PresenterChangeRequest presenterChangeRequest)
        {
            _context.PresenterChangeRequests.Remove(presenterChangeRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PresenterChangeRequest?> GetPresenterChangeRequestByIdAsync(string presenterChangeRequestId)
        {
            return await _context.PresenterChangeRequests.FindAsync(presenterChangeRequestId);
        }

        public async Task<List<PresenterChangeRequest>> GetAllPresenterChangeRequestsAsync()
        {
            return await _context.PresenterChangeRequests.ToListAsync();
        }

        public async Task<List<PresenterChangeRequest>> GetAllPresenterChangeRequestsByConfIdAndStatusIdAsync(string statusId, string confId)
        {
            return await _context.PresenterChangeRequests.Where(pcr => pcr.GlobalStatusId == statusId && pcr.Paper.ConferenceId == confId).ToListAsync();
        }
    }
}