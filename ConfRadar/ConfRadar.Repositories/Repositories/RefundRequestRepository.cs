using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRefundRequestRepository
    {
        Task<int> CreateRefundRequestAsync(RefundRequest refundRequest);
        Task<int> UpdateRefundRequestAsync(RefundRequest refundRequest);
        Task<int> DeleteRefundRequestAsync(RefundRequest refundRequest);
        Task<RefundRequest?> GetRefundRequestByIdAsync(string refundRequestId);
        Task<RefundRequest?> GetRefundRequestByTicketIdAsync(string ticketId);

        Task<List<RefundRequest>> GetAllRefundRequestsAsync();

    }
    public class RefundRequestRepository : GenericRepository<RefundRequest>, IRefundRequestRepository
    {
        public RefundRequestRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateRefundRequestAsync(RefundRequest refundRequest)
        {
            return await CreateAsync(refundRequest);
        }

        public async Task<int> UpdateRefundRequestAsync(RefundRequest refundRequest)
        {
            return await UpdateAsync(refundRequest);
        }

        public async Task<int> DeleteRefundRequestAsync(RefundRequest refundRequest)
        {
            _context.RefundRequests.Remove(refundRequest);
            return await _context.SaveChangesAsync();
        }

        public async Task<RefundRequest?> GetRefundRequestByIdAsync(string refundRequestId)
        {
            return await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.RefundRequestId == refundRequestId);
        }

        public async Task<List<RefundRequest>> GetAllRefundRequestsAsync()
        {
            return await _context.RefundRequests
                .ToListAsync();
        }

        public async Task<RefundRequest?> GetRefundRequestByTicketIdAsync(string ticketId)
        {
            return await _context.RefundRequests.FirstOrDefaultAsync(r => r.TicketId == ticketId);
        }
    }


}
