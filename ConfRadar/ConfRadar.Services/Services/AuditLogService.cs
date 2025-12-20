using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Shared.DTO.AuditLog;

namespace ConfRadar.Services.Services
{
    public interface IAuditLogService
    {
        Task<List<AuditLogDetailResponse>> GetListAuditLogDetail();
        Task<List<AuditLogCategory>> GetAuditLogCategories();
        Task<int> CreateAuditLog(string userId, AuditLogActionNameEnum auditActionEnum, string actionDescription);
        Task<int> GetUserCount();
        Task<int> CountUnResolveReport();
        Task<List<AuditLogDetailResponse>> GetRecentAuditActivity(int? rows = null);
        Task<List<AuditReportDetailResponse>> GetRecentReport(int? rows = null);
        Task<AuditInternalConferenceCountDetailResponse> CountActiveInternalEvents();
        Task<AuditExternalConferenceCountDetailResponse> CountActiveExternalEvents();
        Task<List<GeneralFaq>> GetListGeneralFAQ();


    }
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        public AuditLogService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
        }

        public async Task<List<AuditLogCategory>> GetAuditLogCategories()
        {
            return await _unitOfWork.AuditLogCategoryRepository.GetAllAuditLogCategoriesAsync();
        }

        public async Task<List<AuditLogDetailResponse>> GetListAuditLogDetail()
        {
            var auditLogs = await _unitOfWork.AuditLogRepository.GetAllAuditLogsWithoutTrackingAsync();
            return auditLogs.Select(al => new AuditLogDetailResponse()
            {
                AuditLogId = al.AuditLogId,
                UserId = al.UserId,
                UserFullName = al.User?.FullName,
                UserAvatarUrl = al.User?.AvatarUrl,
                CategoryId = al.CategoryId,
                CategoryName = al.Category?.Name,
                ActionDescription = al.ActionDescription,
                CreatedAt = al.CreatedAt,


            }).ToList();
        }
        public async Task<int> CreateAuditLog(string userId, AuditLogActionNameEnum auditActionEnum, string actionDescription)
        {

            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            var timeNow = await _timeProviderService.GetVietnamTime();
            var auditAction = await _unitOfWork.AuditLogCategoryRepository.GetAuditLogCategoryByNameAsync(auditActionEnum.GetDescription());
            var auditLog = new AuditLog()
            {
                AuditLogId = Guid.NewGuid().ToString(),
                UserId = userId,
                CategoryId = auditAction != null ? auditAction.CategoryId : null,
                ActionDescription = $"Người dùng {user.FullName} đã {actionDescription} vào lúc {timeNow}",
                CreatedAt = timeNow,
            };
            return await _unitOfWork.AuditLogRepository.CreateAuditLogAsync(auditLog);
        }

        public async Task<int> GetUserCount()
        {
            return await _unitOfWork.AuditLogRepository.CountUser();
        }
        public async Task<int> CountUnResolveReport()
        {
            return await _unitOfWork.AuditLogRepository.CountUnResolveReports();
        }
       

      



        public async Task<List<AuditLogDetailResponse>> GetRecentAuditActivity(int? rows = null)
        {
           return await _unitOfWork.AuditLogRepository.GetRecentAuditActivity(rows);
        }

        public async Task<List<AuditReportDetailResponse>> GetRecentReport(int? rows = null)
        {
            return await _unitOfWork.AuditLogRepository.GetRecentReport(rows);
        }

        public async Task<AuditInternalConferenceCountDetailResponse> CountActiveInternalEvents()
        {
            var readyConfStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            if (readyConfStatus == null)
            {
                return new AuditInternalConferenceCountDetailResponse();
            }
            return await _unitOfWork.AuditLogRepository.CountActiveInternalEvents(readyConfStatus.ConferenceStatusId);
        }

        public async Task<AuditExternalConferenceCountDetailResponse> CountActiveExternalEvents()
        {
            var readyConfStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            if (readyConfStatus == null)
            {
                return new AuditExternalConferenceCountDetailResponse();
            }
            return await _unitOfWork.AuditLogRepository.CountActiveExternalEvents(readyConfStatus.ConferenceStatusId);
        }

        public async Task<List<GeneralFaq>> GetListGeneralFAQ()
        {
            return await _unitOfWork.GeneralFAQRepository.GetAllGeneralFAQAsync();
        }
    }
}
