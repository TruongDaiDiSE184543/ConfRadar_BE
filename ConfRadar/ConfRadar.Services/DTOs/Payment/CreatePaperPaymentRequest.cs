using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreatePaperPaymentRequest
    {
        [Required(ErrorMessage = "Mã giá vé  là bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Tiêu đề cho bài báo là bắt buộc")]

        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả cho bài báo là bắt buộc")]

        public string? Description { get; set; }
        [Required(ErrorMessage = "Mã phương thức thanh toán là bắt buộc")]
        public string PaymentMethodId { get; set; }


    }
    public class CreateResearchAttendeePaymentRequest
    {
        [Required(ErrorMessage = "Mã giá vé là bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Mã phương thức thanh toán là bắt buộc")]
        public string PaymentMethodId { get; set; }

    }
}
