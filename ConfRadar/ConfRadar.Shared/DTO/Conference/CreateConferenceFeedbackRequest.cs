using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Conference
{
    public class CreateConferenceFeedbackRequest
    {

        [Required(ErrorMessage = "Mã phiên là bắt buộc")]
        public string ConferenceSessionId { get; set; }
        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; }

        public string? Message { get; set; }

    }
}
