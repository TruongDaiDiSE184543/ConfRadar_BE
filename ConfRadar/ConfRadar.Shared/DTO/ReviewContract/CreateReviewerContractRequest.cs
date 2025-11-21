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
        [Required(ErrorMessage = "Ngày kí hợp đồng là bắt buộc")]

        public DateOnly? SignDay { get; set; }

    }
    public class CreateReviewerContractForNewUserRequest
    {

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email sai format")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 kí tự")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Mật khẩu xác nhận là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu")]
        public string ConfirmPassword { get; set; }




        [Required(ErrorMessage = "Lương cho reviewer là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Lương phải lớn hơn 0")]
        public decimal Wage { get; set; }
        [Required(ErrorMessage = "File hợp đồng là bắt buộc")]
        public IFormFile ContractFile { get; set; }
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]
        public string ConferenceId { get; set; }
        [Required(ErrorMessage = "Ngày kí hợp đồng là bắt buộc")]
        public DateOnly SignDay { get; set; }

    }
}
