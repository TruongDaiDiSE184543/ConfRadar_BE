using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.AuditLog;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAuditLogRepository
    {
        Task<int> CreateAuditLogAsync(AuditLog auditLog);
        Task<AuditLog?> GetAuditLogByIdAsync(string auditLogId);
        Task<List<AuditLog>> GetAllAuditLogsWithoutTrackingAsync();
        Task<List<AuditLog>> GetAuditLogsByUserIdAsync(string userId);
        Task<int> CountUser();
        Task<int> CountUnResolveReports();
        Task<AuditExternalConferenceCountDetailResponse> CountActiveExternalEvents(string confStatusId);
        Task<AuditInternalConferenceCountDetailResponse> CountActiveInternalEvents(string confStatusId);
        Task<List<AuditReportDetailResponse>> GetRecentReport(int? rows = null);
        Task<List<AuditLogDetailResponse>> GetRecentAuditActivity(int? rows = null);
    }
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateAuditLogAsync(AuditLog auditLog)
        {
            return await CreateAsync(auditLog);
        }
        public async Task<int> CreateMutipleAuditLogAsync(List<AuditLog> auditLogs)
        {
            _context.AuditLogs.AddRange(auditLogs);
            return await _context.SaveChangesAsync();
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(string auditLogId)
        {
            return await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.AuditLogId == auditLogId);
        }

        public async Task<List<AuditLog>> GetAllAuditLogsWithoutTrackingAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(al => al.CreatedAt)
                .Include(al => al.Category)
                .Include(al => al.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserIdAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> CountUser()
        {
            return await _context.Users.Where(u=>u.IsEmailConfirmed==true).CountAsync();
        }
        public async Task<int> CountUnResolveReports()
        {
            return await _context.Reports
                .Where(r => r.HasResolve == false)
                .CountAsync();
        }
       
        public async Task<List<AuditLogDetailResponse>> GetRecentAuditActivity(int? rows = null)
        {
            int take = rows.HasValue ? rows.Value : 7;
            var result = await _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt).Take(take)
                .Select(a => new AuditLogDetailResponse()
                {
                    AuditLogId = a.AuditLogId,
                    CreatedAt = a.CreatedAt,
                    ActionDescription = a.ActionDescription,

                    UserId = a.User != null ? a.User.UserId : null,
                    UserFullName = a.User != null ? a.User.FullName : null,
                    UserAvatarUrl = a.User != null ? a.User.AvatarUrl : null,

                    CategoryId = a.Category != null ? a.Category.CategoryId : null,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    
                }).ToListAsync();
            return result;

        }
        public async Task<List<AuditReportDetailResponse>> GetRecentReport(int? rows = null)
        {
            int take = rows.HasValue ? rows.Value : 7;
            var result = await _context.Reports
                .OrderByDescending(r => r.CreatedAt).Take(take)
                .Select(r => new AuditReportDetailResponse()
            {
                ReportId = r.ReportId,
                ReportSubject = r.ReportSubject,
                Reason = r.Reason,
                Description = r.Description,
                HasResolve = r.HasResolve,
                CreatedAt = r.CreatedAt,

                ReportFeedbackId = r.ReportFeedback != null
                ? r.ReportFeedback.ReportId
                : null,

                ReportFeedbackSubject = r.ReportFeedback != null
                ? r.ReportFeedback.ReportSubject
                : null,

                ReportFeedbackReason = r.ReportFeedback != null
                ? r.ReportFeedback.Reason
                : null,

                ReportFeedbackAdminId = r.ReportFeedback != null
                ? r.ReportFeedback.AdminId
                : null,

                ReporterId = r.User!=null ? r.User.UserId:null,
                ReporterFullName = r.User != null ? r.User.FullName : null,
                ReporterEmail = r.User != null ? r.User.Email : null,
                ReporterAvatarUrl = r.User != null ? r.User.AvatarUrl: null,

            }).ToListAsync();
            return result;
        }

        public async Task<AuditExternalConferenceCountDetailResponse> CountActiveExternalEvents(string confStatusId)
        {
            var query = _context.Conferences.Where(c => c.IsInternalHosted == false);

            var result = new AuditExternalConferenceCountDetailResponse()
            {
                TotalExternal = await query.CountAsync(),
                TotalActiveExternalResearch = await query.CountAsync(c=>c.IsResearchConference==true && c.ConferenceStatusId==confStatusId),
                TotalActiveExternalTech = await query.CountAsync(c => c.IsResearchConference == false && c.ConferenceStatusId==confStatusId),
            };
            return result;  

        }
        public async Task<AuditInternalConferenceCountDetailResponse> CountActiveInternalEvents(string confStatusId)
        {
            var query = _context.Conferences.Where(c => c.IsInternalHosted == true);

            var result = new AuditInternalConferenceCountDetailResponse()
            {
                TotalInternal = await query.CountAsync(),
                TotalActiveInternalResearch = await query.CountAsync(c => c.IsResearchConference == true && c.ConferenceStatusId == confStatusId),
                TotalActiveInternalTech = await query.CountAsync(c => c.IsResearchConference == false && c.ConferenceStatusId == confStatusId),
            };
            return result;

        }


        public async Task<int> CountActiveEvents(string confStatusId, bool isInternal)
        {
            return await _context.Conferences
                .Where(c => c.IsInternalHosted == isInternal && c.ConferenceStatusId != null && confStatusId.Contains(c.ConferenceStatusId))
                .CountAsync();
        }
    }

}
