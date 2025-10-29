namespace ConfRadar.Services.DTOs.Payment
{
    public class TransactionDataHolder
    {
        public string TicketId { get; set; }
        public string UserId { get; set; }
        public string PaymentMethodId { get; set; }
        public string ConferencePriceId { get; set; }
        public string ConferenceId { get; set; }
        public List<string> ConferenceSessionIds { get; set; }
       
    }
    //public class TransactionResearchConferenceAbstractDataHolder
    //{
    //    public string TicketId { get; set; }
    //    public string UserId { get; set; }
    //    public string PaymentMethodId { get; set; }
    //    public string ConferencePriceId { get; set; }
    //    public string ConferenceId { get; set; }
    //    public List<string> ConferenceSessionIds { get; set; }
    //}
}
