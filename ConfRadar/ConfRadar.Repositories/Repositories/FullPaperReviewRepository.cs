using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
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
    }
}