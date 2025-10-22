namespace ConfRadar.Services.DTOs.Payment
{
    public class TransactionDataHolder
    {
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        public string TransactionStatusId { get; set; }
        public string TransactionTypeId { get; set; }
        public string PaymentMethodId { get; set; }
        public string ConferencePriceId { get; set; }
        public List<string> ConferenceSessionIds { get; set; }
    }
}
