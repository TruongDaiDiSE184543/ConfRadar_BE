using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.User
{

    public class FirebaseLoginRequest
    {
        [Required(ErrorMessage = "Token is required!")]
        public string Token { get; set; }
        public string? FirebaseWebFcmToken { get; set; }
        public string? FirebaseMobileFcmToken { get; set; }
    }
}
