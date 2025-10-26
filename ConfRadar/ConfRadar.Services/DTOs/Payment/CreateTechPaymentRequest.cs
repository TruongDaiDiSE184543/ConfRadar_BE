using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Payment
{
    public class CreateTechPaymentRequest
    {
        [Required(ErrorMessage = "Price id is required!")]
        public string ConferencePriceId { get; set; }

    }
}
