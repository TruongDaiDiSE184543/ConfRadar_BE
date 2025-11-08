using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.ReviewContract
{
    public class GetUsersForReviewerContractResponse
    {
        public string UserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BioDescription { get; set; }
    }
}
