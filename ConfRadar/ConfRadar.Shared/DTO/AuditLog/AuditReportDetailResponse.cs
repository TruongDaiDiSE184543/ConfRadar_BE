using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.AuditLog
{
    public class AuditReportDetailResponse
    {
        public string ReportId { get; set; } = null!;

        public string? ReportSubject { get; set; }

        public string? Reason { get; set; }

        public string? Description { get; set; }

        public bool? HasResolve { get; set; }

        public DateTime? CreatedAt { get; set; }

       



        public string ReportFeedbackId { get; set; } = null!;

        public string? ReportFeedbackSubject { get; set; }

        public string? ReportFeedbackReason { get; set; }

        public string? ReportFeedbackAdminId { get; set; }




        public string? ReporterAvatarUrl { get; set; }
        public string? ReporterFullName { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterId { get; set; }
    }
}
