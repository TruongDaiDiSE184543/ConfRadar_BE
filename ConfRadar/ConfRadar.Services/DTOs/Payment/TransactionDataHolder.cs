namespace ConfRadar.Services.DTOs.Payment
{
    public class TransactionDataHolder
    {
        public string TicketId { get; set; }
        public string UserId { get; set; }
        public string PaperId { get; set; }
        public string PaymentMethodId { get; set; }
        public string ConferencePriceId { get; set; }
        public string PricePhaseId { get; set; }
        public string ConferenceId { get; set; }
        public string PaymentConferenceLockKey { get; set; }
        public string PaymentPhaseLockKey { get; set; }
        public string ResearchConferencePhaseId { get; set; } = null;
        public List<string> ConferenceSessionIds { get; set; }
        public bool IsResearchConference { get; set; }
        public bool? IsResearchConferenceAuthor { get; set; } = null;
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
