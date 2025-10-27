using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.User
{
    public class UserDetailResponse
    {
        public string UserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BioDescription { get; set; }
        public DateTime? CreatedAt { get; set; }







    }
}
