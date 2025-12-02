using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAuditLogRepository
    {
        Task<int> CreateAuditLogAsync(AuditLog auditLog);
        Task<AuditLog?> GetAuditLogByIdAsync(string auditLogId);
        Task<List<AuditLog>> GetAllAuditLogsWithoutTrackingAsync();
        Task<List<AuditLog>> GetAuditLogsByUserIdAsync(string userId);
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
    }

}
