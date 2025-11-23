using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IOrcidDataCacheRepository
    {
        Task<int> CreateOrcidDataCacheAsync(OrcidDataCache orcidDataCache);
        Task<int> UpdateOrcidDataCacheAsync(OrcidDataCache orcidDataCache);
        Task<bool> DeleteOrcidDataCacheAsync(OrcidDataCache orcidDataCache);
        Task<OrcidDataCache?> GetOrcidDataCacheByIdAsync(string orcidDataCacheId);
        Task<OrcidDataCache?> GetOrcidDataCacheByAcademicProfileIdAndDataTypeAsync(string academicProfileId, string dataType);
        Task<List<OrcidDataCache>> GetOrcidDataCachesByAcademicProfileIdAsync(string academicProfileId);
        Task<List<OrcidDataCache>> GetOrcidDataCachesByDataTypeAsync(string dataType);
        Task<List<OrcidDataCache>> GetAllOrcidDataCachesAsync();
        Task<OrcidDataCache> GetCacheByUserIdAndDataTypeAsync(string userId, string dataType);
    }

    public class OrcidDataCacheRepository : GenericRepository<OrcidDataCache>, IOrcidDataCacheRepository
    {
        public OrcidDataCacheRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateOrcidDataCacheAsync(OrcidDataCache orcidDataCache)
        {
            return await CreateAsync(orcidDataCache);
        }

        public async Task<int> UpdateOrcidDataCacheAsync(OrcidDataCache orcidDataCache)
        {
            return await UpdateAsync(orcidDataCache);
        }

        public async Task<bool> DeleteOrcidDataCacheAsync(OrcidDataCache orcidDataCache)
        {
            return await RemoveAsync(orcidDataCache);
        }

        public async Task<OrcidDataCache?> GetOrcidDataCacheByIdAsync(string orcidDataCacheId)
        {
            return await _context.OrcidDataCaches
                .Include(oc => oc.AcademicProfile)
                .FirstOrDefaultAsync(oc => oc.OrcidDataCacheId == orcidDataCacheId);
        }

        public async Task<OrcidDataCache?> GetOrcidDataCacheByAcademicProfileIdAndDataTypeAsync(string academicProfileId, string dataType)
        {
            return await _context.OrcidDataCaches
                .Include(oc => oc.AcademicProfile)
                .FirstOrDefaultAsync(oc => oc.AcademicProfileId == academicProfileId && oc.DataType.ToLower() == dataType.ToLower());
        }

        public async Task<List<OrcidDataCache>> GetOrcidDataCachesByAcademicProfileIdAsync(string academicProfileId)
        {
            return await _context.OrcidDataCaches
                .Include(oc => oc.AcademicProfile)
                .Where(oc => oc.AcademicProfileId == academicProfileId)
                .ToListAsync();
        }

        public async Task<List<OrcidDataCache>> GetOrcidDataCachesByDataTypeAsync(string dataType)
        {
            return await _context.OrcidDataCaches
                .Include(oc => oc.AcademicProfile)
                .Where(oc => oc.DataType == dataType)
                .ToListAsync();
        }

        public async Task<List<OrcidDataCache>> GetAllOrcidDataCachesAsync()
        {
            return await _context.OrcidDataCaches
                .Include(oc => oc.AcademicProfile)
                .ToListAsync();
        }

        public async Task<OrcidDataCache> GetCacheByUserIdAndDataTypeAsync(string userId, string dataType)
        {
            // Đây là truy vấn LINQ sẽ được EF Core dịch thành câu lệnh SQL JOIN hiệu quả
            return await _context.OrcidDataCaches
                .Include(cache => cache.AcademicProfile) // Dùng Include để JOIN
                .FirstOrDefaultAsync(cache =>
                    cache.AcademicProfile.UserId == userId && // Lọc theo UserId từ bảng cha
                    cache.DataType == dataType &&             // Lọc theo DataType từ bảng con
                    cache.AcademicProfile.Scope == "read-limited" // Đảm bảo profile đó có quyền đọc
                );
        }
    }
}