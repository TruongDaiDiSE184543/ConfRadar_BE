namespace ConfRadar.Services.DTOs.Transaction
{
    public class TransactionDetailResponse
    {
        public string TransactionId { get; set; } = null!;
        public string? Currency { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? TransactionCode { get; set; }
        public bool? IsRefunded { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? TicketId { get; set; }

    }
}
