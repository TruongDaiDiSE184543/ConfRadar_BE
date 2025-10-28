using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReviewStatusRepository
    {
        Task<int> CreateReviewStatusAsync(ReviewStatus status);
        Task<int> CreateMultipleReviewStatusesAsync(List<ReviewStatus> statuses);
        Task<int> UpdateReviewStatusAsync(ReviewStatus status);
        Task<bool> DeleteReviewStatusAsync(ReviewStatus status);
        Task<ReviewStatus?> GetReviewStatusByIdAsync(string statusId);
        Task<ReviewStatus?> GetReviewStatusByNameAsync(string statusName);
        Task<List<ReviewStatus>> GetAllReviewStatusesAsync();
    }
    public class ReviewStatusRepository : GenericRepository<ReviewStatus>, IReviewStatusRepository
    {
        public ReviewStatusRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateReviewStatusAsync(ReviewStatus status)
        {
            return await CreateAsync(status);
        }

        public async Task<int> CreateMultipleReviewStatusesAsync(List<ReviewStatus> statuses)
        {
            await _context.ReviewStatuses.AddRangeAsync(statuses);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateReviewStatusAsync(ReviewStatus status)
        {
            return await UpdateAsync(status);
        }

        public async Task<bool> DeleteReviewStatusAsync(ReviewStatus status)
        {
            return await RemoveAsync(status);
        }

        public async Task<ReviewStatus?> GetReviewStatusByIdAsync(string statusId)
        {
            return await _context.ReviewStatuses
                .FirstOrDefaultAsync(s => s.ReviewStatusId == statusId);
        }

        public async Task<ReviewStatus?> GetReviewStatusByNameAsync(string statusName)
        {
            return await _context.ReviewStatuses
                .FirstOrDefaultAsync(s => s.Name == statusName);
        }

        public async Task<List<ReviewStatus>> GetAllReviewStatusesAsync()
        {
            return await _context.ReviewStatuses.ToListAsync();
        }
    }
}
