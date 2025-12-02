using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.User
{
    public class UserSuspendRequest
    {
        [Required(ErrorMessage = "Mã người dùng là bắt buộc")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Lí do là bắt buộc")]
        public string Reason { get; set; }
    }
    public class UserActiveAccountRequest
    {
        [Required(ErrorMessage = "Mã người dùng là bắt buộc")]
        public string UserId { get; set; }
    }
}
