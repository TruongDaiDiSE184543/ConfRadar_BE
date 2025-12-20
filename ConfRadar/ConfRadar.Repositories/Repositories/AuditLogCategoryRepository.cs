using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IAuditLogCategoryRepository
    {
        Task<int> CreateAuditLogCategoryAsync(AuditLogCategory category);
        Task<int> UpdateAuditLogCategoryAsync(AuditLogCategory category);
        Task<bool> DeleteAuditLogCategoryAsync(AuditLogCategory category);
        Task<AuditLogCategory?> GetAuditLogCategoryByIdAsync(string categoryId);
        Task<AuditLogCategory?> GetAuditLogCategoryByNameAsync(string categoryName);
        Task<List<AuditLogCategory>> GetAllAuditLogCategoriesAsync();
        Task<int> CreateMultipleAuditLogCategoriesAsync(List<AuditLogCategory> categories);
    }
    public class AuditLogCategoryRepository : GenericRepository<AuditLogCategory>, IAuditLogCategoryRepository
    {
        public AuditLogCategoryRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateAuditLogCategoryAsync(AuditLogCategory category)
        {
            return await CreateAsync(category);
        }

        public async Task<int> UpdateAuditLogCategoryAsync(AuditLogCategory category)
        {
            return await UpdateAsync(category);
        }

        public async Task<bool> DeleteAuditLogCategoryAsync(AuditLogCategory category)
        {
            return await RemoveAsync(category);
        }

        public async Task<AuditLogCategory?> GetAuditLogCategoryByIdAsync(string categoryId)
        {
            return await _context.AuditLogCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<AuditLogCategory?> GetAuditLogCategoryByNameAsync(string categoryName)
        {
            return await _context.AuditLogCategories
                .FirstOrDefaultAsync(c => c.Name == categoryName);
        }

        public async Task<List<AuditLogCategory>> GetAllAuditLogCategoriesAsync()
        {
            return await GetAllAsync();
        }

        public async Task<int> CreateMultipleAuditLogCategoriesAsync(List<AuditLogCategory> categories)
        {
            await _context.AuditLogCategories.AddRangeAsync(categories);
            return await _context.SaveChangesAsync();
        }



    }
}
