using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.DTO.User
{
    public class AvailableCustomerResponse
    {
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }


    }
}
