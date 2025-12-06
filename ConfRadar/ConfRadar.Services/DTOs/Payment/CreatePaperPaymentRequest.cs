using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreatePaperPaymentRequest
    {
        [Required(ErrorMessage = "Mã giá vé  là bắt buộc")]
        public string ConferencePriceId { get; set; }

        [Required(ErrorMessage = "Mã phương thức thanh toán là bắt buộc")]
        public string PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; }


    }
    public class CreateResearchAttendeePaymentRequest
    {
        [Required(ErrorMessage = "Mã giá vé là bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Mã phương thức thanh toán là bắt buộc")]
        public string PaymentMethodId { get; set; }

    }
}
