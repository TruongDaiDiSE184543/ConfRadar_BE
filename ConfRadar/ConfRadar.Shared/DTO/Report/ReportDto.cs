using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Report
{
    // Report DTOs
    public class CreateReportRequest
    {
        [Required(ErrorMessage = "Tiêu đề báo cáo là bắt buộc")]
        [MaxLength(255)]
        public string? ReportSubject { get; set; }

        [Required(ErrorMessage = "Lý do báo cáo là bắt buộc")]
        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class CreateReportFeedbackRequest
    {
        [Required(ErrorMessage = "Nội dung phản hồi là bắt buộc")]
        [MaxLength(1000)]
        public string? ReportSubject { get; set; }

        [Required(ErrorMessage = "Lý do phản hồi là bắt buộc")]
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class ReportResponse
    {
        public string ReportId { get; set; }
        public string? ReportSubject { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public bool? HasResolve { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserId { get; set; }
        public UserResponse? User { get; set; }
        public ReportFeedbackResponse? ReportFeedback { get; set; }
    }

    public class ReportFeedbackResponse
    {
        public string ReportId { get; set; }
        public string? ReportSubject { get; set; }
        public string? Reason { get; set; }
        public string? AdminId { get; set; }
        public UserResponse? Admin { get; set; }
    }

    public class UnresolvedReportResponse
    {
        public string ReportId { get; set; }
        public string? ReportSubject { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserId { get; set; }
        public UserResponse? User { get; set; }
    }

    public class UserResponse
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}
