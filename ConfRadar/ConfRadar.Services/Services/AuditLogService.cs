using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.AuditLog;

namespace ConfRadar.Services.Services
{
    public interface IAuditLogService
    {
        Task<List<AuditLogDetailResponse>> GetListAuditLogDetail();
        Task<List<AuditLogCategory>> GetAuditLogCategories();
    }
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
