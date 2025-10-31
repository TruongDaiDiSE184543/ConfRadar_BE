using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRankingCategoryRepository
    {
        Task<RankingCategory?> GetRankingCategoryByName(string rankName);
        Task<int> CreateMultipleRankingCategoriesAsync(IEnumerable<RankingCategory> rankingCategories);
        Task<int> CreateRankingCategory(RankingCategory rankingCategory);
        Task<RankingCategory?> GetRankingCategoryByIdAsync(string rankingCategoryId);
        Task<int> UpdateRankingCategoryAsync(RankingCategory rankingCategory);
        Task<bool> DeleteRankingCategoryAsync(RankingCategory rankingCategory);
        Task<List<RankingCategory>> GetAllRankingCategoryAsync();
    }

    public class RankingCategoryRepository : GenericRepository<RankingCategory>, IRankingCategoryRepository
    {
        public RankingCategoryRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<RankingCategory?> GetRankingCategoryByName(string rankName)
        {
            return await _context.RankingCategories.FirstOrDefaultAsync(x => x.RankName == rankName);
        }

        public async Task<int> CreateMultipleRankingCategoriesAsync(IEnumerable<RankingCategory> rankingCategories)
        {
            await _context.RankingCategories.AddRangeAsync(rankingCategories);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateRankingCategory(RankingCategory rankingCategory)
        {
            return await CreateAsync(rankingCategory);
        }

        public async Task<RankingCategory?> GetRankingCategoryByIdAsync(string rankingCategoryId)
        {
            //return await GetByIdAsync(rankingCategoryId);
            return await _context.RankingCategories.FirstOrDefaultAsync(x => x.RankingCategoryId == rankingCategoryId);
        }

        public async Task<int> UpdateRankingCategoryAsync(RankingCategory rankingCategory)
        {
            return await UpdateAsync(rankingCategory);
        }

        public async Task<bool> DeleteRankingCategoryAsync(RankingCategory rankingCategory)
        {
            return await RemoveAsync(rankingCategory);
        }

        public async Task<List<RankingCategory>> GetAllRankingCategoryAsync()
        {
            return await GetAllAsync();
        }
    }
}