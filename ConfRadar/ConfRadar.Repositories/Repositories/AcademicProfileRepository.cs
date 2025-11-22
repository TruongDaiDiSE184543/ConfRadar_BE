using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAcademicProfileRepository
    {
        Task<int> CreateAcademicProfileAsync(AcademicProfile academicProfile);
        Task<int> UpdateAcademicProfileAsync(AcademicProfile academicProfile);
        Task<bool> DeleteAcademicProfileAsync(AcademicProfile academicProfile);
        Task<AcademicProfile?> GetAcademicProfileByIdAsync(string academicProfileId);
        Task<AcademicProfile?> GetAcademicProfileByUserIdAsync(string userId);
        Task<AcademicProfile?> GetAcademicProfileByUserIdAndScopeAsync(string userId, string scope);
        Task<AcademicProfile?> GetAcademicProfileByOrcidAndScopeAsync(string orcidId, string scope);
        Task<List<AcademicProfile>> GetAcademicProfilesByUserIdAsync(string userId);
        Task<List<AcademicProfile>> GetAcademicProfilesByScopeAsync(string scope);
        Task<List<AcademicProfile>> GetAllAcademicProfilesAsync();
    }

    public class AcademicProfileRepository : GenericRepository<AcademicProfile>, IAcademicProfileRepository
    {
        public AcademicProfileRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateAcademicProfileAsync(AcademicProfile academicProfile)
        {
            return await CreateAsync(academicProfile);
        }

        public async Task<int> UpdateAcademicProfileAsync(AcademicProfile academicProfile)
        {
            return await UpdateAsync(academicProfile);
        }

        public async Task<bool> DeleteAcademicProfileAsync(AcademicProfile academicProfile)
        {
            return await RemoveAsync(academicProfile);
        }

        public async Task<AcademicProfile?> GetAcademicProfileByIdAsync(string academicProfileId)
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .FirstOrDefaultAsync(ap => ap.AcademicProfileId == academicProfileId);
        }

        public async Task<AcademicProfile?> GetAcademicProfileByUserIdAsync(string userId)
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .FirstOrDefaultAsync(ap => ap.UserId == userId);
        }

        public async Task<AcademicProfile?> GetAcademicProfileByUserIdAndScopeAsync(string userId, string scope)
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .FirstOrDefaultAsync(ap => ap.UserId == userId && ap.Scope == scope);
        }

        public async Task<List<AcademicProfile>> GetAcademicProfilesByUserIdAsync(string userId)
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .Where(ap => ap.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<AcademicProfile>> GetAcademicProfilesByScopeAsync(string scope)
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .Where(ap => ap.Scope == scope)
                .ToListAsync();
        }

        public async Task<List<AcademicProfile>> GetAllAcademicProfilesAsync()
        {
            return await _context.AcademicProfiles
                .Include(ap => ap.User)
                .ToListAsync();
        }

        public async Task<AcademicProfile?> GetAcademicProfileByOrcidAndScopeAsync(string orcidId, string scope)
        {
            return await _context.AcademicProfiles
               .Include(ap => ap.User)
               .FirstOrDefaultAsync(ap => ap.OrcidId == orcidId && ap.Scope == scope);
        }
    }
}