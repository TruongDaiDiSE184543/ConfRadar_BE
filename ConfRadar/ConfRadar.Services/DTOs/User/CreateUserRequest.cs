using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace ConfRadar.Services.DTOs.User
{
    public class CreateUserRequest
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }


        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(255)]
        public string FullName { get; set; }

        public DateOnly? Birthday { get; set; }


        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }


        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? BioDescription { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
