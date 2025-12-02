using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Notification
{
    public class UpdateReadStatusRequest
    {

        [Required(ErrorMessage = "Mã thông báo là bắt buộc")]
        public string NotificationId { get; set; } = null!;
        [Required(ErrorMessage = "Trạng thái đọc là bắt buộc")]
        public bool? ReadStatus { get; set; }
    }
}
