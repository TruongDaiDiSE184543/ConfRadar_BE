using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ConfRadar.Shared.DTO.RefundRequest
{
    public class RefundRequestResponse
    {
        public string? RefundRequestId { get; set; } 
        public string? TransactionId { get; set; }
        public string? TicketId { get; set; }
        public RefundTicketDetailResponse Ticket { get; set; } = new RefundTicketDetailResponse();
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? Reason { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
    public class RefundTicketDetailResponse
    {
        public string? TicketId { get; set; } 
        public DateOnly? RegisteredDate { get; set; }
        public bool? IsRefunded { get; set; }
        public decimal? ActualPrice { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PricePhaseId { get; set; }
        public string? PricePhaseName { get; set; }
        public DateOnly? PricePhaseStartDate { get; set; }
        public DateOnly? PricePhaseEndDate { get; set; }
        public decimal? PricePhaseApplyPercent { get; set; }
        public int? PricePhaseTotalSlot { get; set; }
        public int? PricePhaseAvailableSlot { get; set; }
    }
}
