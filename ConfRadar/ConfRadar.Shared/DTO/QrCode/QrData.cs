namespace ConfRadar.Shared.DTO.QrCode
{
    public class QrDataPayload
    {
        public string? UserCheckinId { get; set; }
        public string? UserId { get; set; }
        public string? TicketId { get; set; }
        public string? ConferenceSessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Signature { get; set; }
    }
    public class VerifyQrDataRequest
    {
        public string Content { get; set; }
    }
}
