using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.RefundRequest;
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
        Task<List<RefundRequestResponse>> GetRefundRequestByConferenceId(string conferenceId);
        Task<List<RefundRequestResponse>> GetAllRefundRequest();

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

        public async Task<List<RefundRequestResponse>> GetRefundRequestByConferenceId(string conferenceId)
        {
            var refundRequests = await _context.RefundRequests
                .AsNoTracking()
                .Where(r => r.Ticket!=null && r.Ticket.PricePhase!=null 
                && r.Ticket.PricePhase.ConferencePrice!=null
                && r.Ticket.PricePhase.ConferencePrice.ConferenceId == conferenceId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RefundRequestResponse()
                {
                    RefundRequestId = r.RefundRequestId,
                    TransactionId = r.TransactionId,
                    TicketId = r.TicketId,
                    GlobalStatusId = r.GlobalStatusId,
                    GlobalStatusName = r.GlobalStatus != null ? r.GlobalStatus.Name : null,
                    Reason = r.Reason,
                    CreatedAt = r.CreatedAt,
                    Ticket = new RefundTicketDetailResponse()
                    {
                        TicketId = r.TicketId,
                        RegisteredDate = r.Ticket !=null ? r.Ticket.RegisteredDate :null,
                        IsRefunded = r.Ticket != null ? r.Ticket.IsRefunded : null,
                        ActualPrice = r.Ticket != null ? r.Ticket.ActualPrice : null,
                        UserId = r.Ticket != null ? r.Ticket.UserId : null,
                        AvatarUrl =  r.Ticket != null && r.Ticket.User != null ? r.Ticket.User.AvatarUrl : null,
                        PricePhaseId = r.Ticket != null ? r.Ticket.PricePhaseId: null,
                        PricePhaseName = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.PhaseName : null,
                        PricePhaseStartDate = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.StartDate : null,
                        PricePhaseEndDate = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.EndDate : null,
                        PricePhaseApplyPercent = r.Ticket != null && r.Ticket.PricePhase!=null ? r.Ticket.PricePhase.ApplyPercent : null,
                        PricePhaseTotalSlot = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.TotalSlot : null,
                        PricePhaseAvailableSlot = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.AvailableSlot : null,
                    }
                }).ToListAsync();
            return refundRequests;
        }

        public async Task<List<RefundRequestResponse>> GetAllRefundRequest()
        {
            var refundRequests = await _context.RefundRequests
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RefundRequestResponse()
                {
                    RefundRequestId = r.RefundRequestId,
                    TransactionId = r.TransactionId,
                    TicketId = r.TicketId,
                    GlobalStatusId = r.GlobalStatusId,
                    GlobalStatusName = r.GlobalStatus != null ? r.GlobalStatus.Name : null,
                    Reason = r.Reason,
                    CreatedAt = r.CreatedAt,
                    Ticket = new RefundTicketDetailResponse()
                    {
                        TicketId = r.TicketId,
                        RegisteredDate = r.Ticket != null ? r.Ticket.RegisteredDate : null,
                        IsRefunded = r.Ticket != null ? r.Ticket.IsRefunded : null,
                        ActualPrice = r.Ticket != null ? r.Ticket.ActualPrice : null,
                        UserId = r.Ticket != null ? r.Ticket.UserId : null,
                        AvatarUrl = r.Ticket != null && r.Ticket.User != null ? r.Ticket.User.AvatarUrl : null,
                        PricePhaseId = r.Ticket != null ? r.Ticket.PricePhaseId : null,
                        PricePhaseName = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.PhaseName : null,
                        PricePhaseStartDate = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.StartDate : null,
                        PricePhaseEndDate = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.EndDate : null,
                        PricePhaseApplyPercent = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.ApplyPercent : null,
                        PricePhaseTotalSlot = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.TotalSlot : null,
                        PricePhaseAvailableSlot = r.Ticket != null && r.Ticket.PricePhase != null ? r.Ticket.PricePhase.AvailableSlot : null,
                    }
                }).ToListAsync();
            return refundRequests;
        }
    }


}
