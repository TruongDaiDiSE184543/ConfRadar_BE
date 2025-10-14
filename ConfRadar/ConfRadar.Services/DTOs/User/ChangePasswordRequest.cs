using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.User
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Change password token is required.")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
