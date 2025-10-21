using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceCategoryRepository
    {
        Task<int> CreateConferenceCategoryAsync(ConferenceCategory category);
        Task<int> UpdateConferenceCategoryAsync(ConferenceCategory category);
        Task<int> DeleteConferenceCategoryAsync(ConferenceCategory category);
        Task<ConferenceCategory?> GetConferenceCategoryByIdAsync(string categoryId);
        Task<ConferenceCategory?> GetCategoryByCategoryName(string categoryName);
        Task<List<ConferenceCategory>> GetAllConferenceCategoriesAsync();
    }

    public class ConferenceCategoryRepository : GenericRepository<ConferenceCategory>, IConferenceCategoryRepository
    {
        public ConferenceCategoryRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceCategoryAsync(ConferenceCategory category)
        {
            return await CreateAsync(category);
        }

        public async Task<int> UpdateConferenceCategoryAsync(ConferenceCategory category)
        {
            return await UpdateAsync(category);
        }

        public async Task<int> DeleteConferenceCategoryAsync(ConferenceCategory category)
        {
            _context.ConferenceCategories.Remove(category);
            return await _context.SaveChangesAsync();
        }

        public async Task<ConferenceCategory?> GetConferenceCategoryByIdAsync(string categoryId)
        {
            return await _context.ConferenceCategories
                .FirstOrDefaultAsync(c => c.ConferenceCategoryId == categoryId);
        }

        public async Task<ConferenceCategory?> GetCategoryByCategoryName(string categoryName)
        {
            return await _context.ConferenceCategories
                .FirstOrDefaultAsync(c => c.ConferenceCategoryName == categoryName);
        }

        public async Task<List<ConferenceCategory>> GetAllConferenceCategoriesAsync()
        {
            return await _context.ConferenceCategories.ToListAsync();
        }
    }
}