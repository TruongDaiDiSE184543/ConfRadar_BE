using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Ticket
{
    public class RefundTicketRequest
    {
        [Required(ErrorMessage = "Mã vé là bắt buộc")]
        public string TicketId { get; set; }
        [Required(ErrorMessage = "Mã giao dịch là bắt buộc")]
        public string TransactionId { get; set; }
    }
}
