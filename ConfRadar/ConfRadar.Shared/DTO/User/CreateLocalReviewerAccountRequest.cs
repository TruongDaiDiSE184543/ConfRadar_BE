using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.User
{
    public class CreateLocalReviewerAccountRequest
    {

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email sai format")]
        [MaxLength(255)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 kí tự")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Mật khẩu xác nhận là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string FullName { get; set; }



    }
}
