using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreateTechPaymentRequest
    {
        [Required(ErrorMessage = "Mã vé là bắt buộc")]
        public string ConferencePriceId { get; set; }
        [Required(ErrorMessage = "Mã phương thức thanh toán là bắt buộc")]
        public string PaymentMethodId { get; set; }

    }
}
