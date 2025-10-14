using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.User
{
    public class LocalLoginUserRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is not a valid email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
