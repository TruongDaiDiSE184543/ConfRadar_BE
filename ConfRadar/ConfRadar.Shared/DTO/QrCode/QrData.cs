using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.QrCode
{
    public class QrDataPayload
    {
        public string userCheckinId { get; set; }
        public string userId { get; set; }
        public string ticketId { get; set; }
        public string conferenceSessionId { get; set; }
        public DateTime createAt { get; set; }
        public string signature { get; set; }
    }
    public class VerifyQrDataRequest
    {
        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Mã session là bắt buộc")]
        public string ConferenceSessionId { get; set; }
    }
}
