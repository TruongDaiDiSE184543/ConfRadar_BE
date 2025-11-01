using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreatePaperPaymentRequest
    {
        [Required(ErrorMessage = "Giá vé id là bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Tiêu đề cho bài báo là bắt buộc")]

        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả cho bài báo là bắt buộc")]

        public string? Description { get; set; }

        //[Required(ErrorMessage = "Payment method bắt buộc")]
        //[EnumDataType(typeof(PaymentMethodEnum))]
        //public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
