using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class CreateReviewerContractRequest
    {
        [Required(ErrorMessage = "Mã reviewer là bắt buộc")]
        public string ReviewerId { get; set; }
        [Required(ErrorMessage = "Lương cho reviewer là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Lương phải lớn hơn 0")]
        public decimal Wage { get; set; }
        [Required(ErrorMessage = "File hợp đồng là bắt buộc")]
        public IFormFile ContractFile { get; set; }
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }

    }
}
