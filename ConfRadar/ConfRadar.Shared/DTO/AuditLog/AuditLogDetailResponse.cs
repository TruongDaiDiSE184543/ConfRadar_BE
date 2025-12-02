using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.AuditLog
{
    public class AuditLogDetailResponse
    {
        public string AuditLogId { get; set; }

        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserAvatarUrl { get; set; }

        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; } 

        public string? ActionDescription { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
