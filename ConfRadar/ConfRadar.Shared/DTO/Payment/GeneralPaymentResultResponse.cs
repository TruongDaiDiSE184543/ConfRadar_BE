namespace ConfRadar.Shared.DTO.Payment
{
    public class GeneralPaymentResultResponse
    {
        public bool PaymentCreateSuccess { get; set; }
        public string PaymentMessage { get; set; }
        public string? CheckOutUrl { get; set; }
        //public bool? IsAddedWaitList { get; set; } = null;
        //public string? ConferenceId { get; set; }


    }
}
