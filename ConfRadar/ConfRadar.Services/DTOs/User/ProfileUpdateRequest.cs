using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.User
{
    public class ProfileUpdateRequest
    {
        public string? FullName { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string? PhoneNumber { get; set; }
        [EnumDataType(typeof(GenderTypeEnum), ErrorMessage = "Invalid gender type")]
        public GenderTypeEnum? Gender { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? BioDescription { get; set; }
    }
}
