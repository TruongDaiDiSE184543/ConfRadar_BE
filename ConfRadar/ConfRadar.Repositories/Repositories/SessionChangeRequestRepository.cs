using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ISessionChangeRequestRepository
    {
        Task<int> CreateSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest);
        Task<int> UpdateSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest);
        Task<bool> DeleteSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest);
        Task<SessionChangeRequest?> GetSessionChangeRequestByIdAsync(string sessionChangeRequestId);
        Task<List<SessionChangeRequest>> GetAllSessionChangeRequestsAsync();
    }

    public class SessionChangeRequestRepository : GenericRepository<SessionChangeRequest>, ISessionChangeRequestRepository
    {
        private readonly ConfRadarDbContext _context;

        public SessionChangeRequestRepository(ConfRadarDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CreateSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest)
        {
            _context.SessionChangeRequests.Add(sessionChangeRequest);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest)
        {
            var tracker = _context.Attach(sessionChangeRequest);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteSessionChangeRequestAsync(SessionChangeRequest sessionChangeRequest)
        {
            _context.SessionChangeRequests.Remove(sessionChangeRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SessionChangeRequest?> GetSessionChangeRequestByIdAsync(string sessionChangeRequestId)
        {
            return await _context.SessionChangeRequests.FindAsync(sessionChangeRequestId);
        }

        public async Task<List<SessionChangeRequest>> GetAllSessionChangeRequestsAsync()
        {
            return await _context.SessionChangeRequests.ToListAsync();
        }
    }
}