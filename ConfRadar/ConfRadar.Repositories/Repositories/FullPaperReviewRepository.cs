using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IFullPaperReviewRepository
    {
        Task<int> CreateFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<int> UpdateFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<bool> DeleteFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<FullPaperReview?> GetFullPaperReviewByIdAsync(string fullPaperReviewId);
        Task<List<FullPaperReview>> GetAllFullPaperReviewsAsync();
        Task<List<FullPaperReview>> GetFullPaperReviewsByFullPaperIdAsync(string fullPaperId);
        Task<List<FullPaperReview>> GetFullPaperReviewsByReviewerIdAsync(string reviewerId);
        Task<FullPaperReview?> GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(string fullPaperId, string reviewerId);
        Task<List<FullPaperReview>> GetReviewsByUserAndPaperIdsAsync(string userId, List<string> paperIds);
    }
    public class FullPaperReviewRepository : GenericRepository<FullPaperReview>, IFullPaperReviewRepository
    {
        public FullPaperReviewRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateFullPaperReviewAsync(FullPaperReview fullPaperReview)
        {
            return await CreateAsync(fullPaperReview);
        }

        public async Task<int> UpdateFullPaperReviewAsync(FullPaperReview fullPaperReview)
        {
            return await UpdateAsync(fullPaperReview);
        }

        public async Task<bool> DeleteFullPaperReviewAsync(FullPaperReview fullPaperReview)
        {
            return await RemoveAsync(fullPaperReview);
        }

        public async Task<FullPaperReview?> GetFullPaperReviewByIdAsync(string fullPaperReviewId)
        {
            //return await GetByIdAsync(fullPaperReviewId);
            return await _context.FullPaperReviews.FirstOrDefaultAsync(x => x.FullPaperReviewId == fullPaperReviewId);
        }

        public async Task<List<FullPaperReview>> GetAllFullPaperReviewsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<FullPaperReview>> GetFullPaperReviewsByFullPaperIdAsync(string fullPaperId)
        {
            return await _context.FullPaperReviews
                .Include(fpr => fpr.Reviewer)
                .Include(fpr => fpr.ReviewStatus)
                .Where(fpr => fpr.FullPaperId == fullPaperId)
                .ToListAsync();
        }

        public async Task<List<FullPaperReview>> GetFullPaperReviewsByReviewerIdAsync(string reviewerId)
        {
            return await _context.FullPaperReviews
                .Include(fpr => fpr.FullPaper)
                .Include(fpr => fpr.ReviewStatus)
                .Where(fpr => fpr.ReviewerId == reviewerId)
                .ToListAsync();
        }

        public async Task<FullPaperReview?> GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(string fullPaperId, string reviewerId)
        {
            return await _context.FullPaperReviews
                .Include(fpr => fpr.Reviewer)
                .Include(fpr => fpr.ReviewStatus)
                .Include(fpr => fpr.FullPaper)
                .FirstOrDefaultAsync(fpr => fpr.FullPaperId == fullPaperId && fpr.ReviewerId == reviewerId);
        }

        public async Task<List<FullPaperReview>> GetReviewsByUserAndPaperIdsAsync(string userId, List<string> paperIds)
        {
            if (paperIds == null || !paperIds.Any())
                return new List<FullPaperReview>();

            return await _context.FullPaperReviews.AsNoTracking()

                .Include(r => r.ReviewStatus)

                .Where(r => r.ReviewerId == userId &&
                            r.FullPaper.Papers.Any(p => paperIds.Contains(p.PaperId)))
                .ToListAsync();
        }
    }
}