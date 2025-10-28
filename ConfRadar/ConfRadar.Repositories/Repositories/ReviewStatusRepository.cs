using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReviewStatusRepository
    {
        Task<ReviewStatus?> GetReviewStatusByName(string name);
        Task<int> CreateMultipleReviewStatusesAsync(IEnumerable<ReviewStatus> reviewStatuses);
        Task<int> CreateReviewStatus(ReviewStatus reviewStatus);
        Task<ReviewStatus> GetReviewStatusByIdAsync(string reviewStatusId);
        Task<int> UpdateReviewStatusAsync(ReviewStatus reviewStatus);
        Task<bool> DeleteReviewStatusAsync(ReviewStatus reviewStatus);
        Task<List<ReviewStatus>> GetAllReviewStatusAsync();
    }

    public class ReviewStatusRepository : GenericRepository<ReviewStatus>, IReviewStatusRepository
    {
        public ReviewStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<ReviewStatus?> GetReviewStatusByName(string name)
        {
            return await _context.ReviewStatuses.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<int> CreateMultipleReviewStatusesAsync(IEnumerable<ReviewStatus> reviewStatuses)
        {
            await _context.ReviewStatuses.AddRangeAsync(reviewStatuses);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateReviewStatus(ReviewStatus reviewStatus)
        {
            return await CreateAsync(reviewStatus);
        }

        public async Task<ReviewStatus> GetReviewStatusByIdAsync(string reviewStatusId)
        {
            return await GetByIdAsync(reviewStatusId);
        }

        public async Task<int> UpdateReviewStatusAsync(ReviewStatus reviewStatus)
        {
            return await UpdateAsync(reviewStatus);
        }

        public async Task<bool> DeleteReviewStatusAsync(ReviewStatus reviewStatus)
        {
            return await RemoveAsync(reviewStatus);
        }

        public async Task<List<ReviewStatus>> GetAllReviewStatusAsync()
        {
            return await GetAllAsync();
        }
    }
}