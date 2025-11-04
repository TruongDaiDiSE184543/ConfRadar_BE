using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.ConferenceStep;

namespace ConfRadar.Services.Services
{
    public interface IReportService
    {
        Task<ReportResponse> CreateReportAsync(string userId, CreateReportRequest request);
        Task<List<UnresolvedReportResponse>> GetUnresolvedReportsAsync();
        Task<ReportFeedbackResponse> CreateReportFeedbackAsync(string reportId, string adminId, CreateReportFeedbackRequest request);
    }

    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReportResponse> CreateReportAsync(string userId, CreateReportRequest request)
        {
            var report = new Report
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportSubject = request.ReportSubject,
                Reason = request.Reason,
                Description = request.Description,
                HasResolve = true, // Default to true as per requirement
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _unitOfWork.ReportRepository.CreateReportAsync(report);

            var createdReport = await _unitOfWork.ReportRepository.GetReportByIdAsync(report.ReportId);
            return MapToReportResponse(createdReport);
        }

        public async Task<List<UnresolvedReportResponse>> GetUnresolvedReportsAsync()
        {
            var reports = await _unitOfWork.ReportRepository.GetUnresolvedReportsAsync();
            return reports.Select(r => new UnresolvedReportResponse
            {
                ReportId = r.ReportId,
                ReportSubject = r.ReportSubject,
                Reason = r.Reason,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId,
                User = r.User != null ? new UserResponse
                {
                    UserId = r.User.UserId,
                    UserName = r.User.FullName,
                    Email = r.User.Email,
                    FullName = r.User.FullName
                } : null
            }).ToList();
        }

        public async Task<ReportFeedbackResponse> CreateReportFeedbackAsync(string reportId, string adminId, CreateReportFeedbackRequest request)
        {
            // First, update the report to mark it as resolved
            var report = await _unitOfWork.ReportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                throw new Exception($"Report with ID {reportId} not found");
            }

            report.HasResolve = true; // Mark the report as resolved
            await _unitOfWork.ReportRepository.UpdateReportAsync(report);

            // Create the report feedback
            var reportFeedback = new ReportFeedback
            {
                ReportId = reportId,
                ReportSubject = request.ReportSubject,
                Reason = request.Reason,
                AdminId = adminId
            };

            await _unitOfWork.ReportFeedbackRepository.CreateReportFeedbackAsync(reportFeedback);

            var createdFeedback = await _unitOfWork.ReportFeedbackRepository.GetReportFeedbackByIdAsync(reportId);
            return MapToReportFeedbackResponse(createdFeedback);
        }

        private ReportResponse MapToReportResponse(Report report)
        {
            return new ReportResponse
            {
                ReportId = report.ReportId,
                ReportSubject = report.ReportSubject,
                Reason = report.Reason,
                Description = report.Description,
                HasResolve = report.HasResolve,
                CreatedAt = report.CreatedAt,
                UserId = report.UserId,
                User = report.User != null ? new UserResponse
                {
                    UserId = report.User.UserId,
                    UserName = report.User.FullName,
                    Email = report.User.Email,
                    FullName = report.User.FullName
                } : null,
                ReportFeedback = report.ReportFeedback != null ? MapToReportFeedbackResponse(report.ReportFeedback) : null
            };
        }

        private ReportFeedbackResponse MapToReportFeedbackResponse(ReportFeedback feedback)
        {
            return new ReportFeedbackResponse
            {
                ReportId = feedback.ReportId,
                ReportSubject = feedback.ReportSubject,
                Reason = feedback.Reason,
                AdminId = feedback.AdminId,
                Admin = feedback.Admin != null ? new UserResponse
                {
                    UserId = feedback.Admin.UserId,
                    //UserName = feedback.Admin.FullName,
                    Email = feedback.Admin.Email,
                    FullName = feedback.Admin.FullName
                } : null,
                Report = feedback.Report != null ? MapToReportResponse(feedback.Report) : null
            };
        }
    }
}