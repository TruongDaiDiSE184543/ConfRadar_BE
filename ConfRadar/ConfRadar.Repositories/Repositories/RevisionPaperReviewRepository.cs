//using ConfRadar.Repositories.Base;
//using ConfRadar.Repositories.Data;
//using ConfRadar.Repositories.Models;
//using Microsoft.EntityFrameworkCore;

//namespace ConfRadar.Repositories.Repositories
//{

//    public interface IRevisionPaperReviewRepository
//    {
//        Task<int> CreateMultipleRevisionPaperReviewsAsync(List<RevisionPaperReview> revisionPaperReviews);
//        Task<int> CreateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview);
//        Task<List<RevisionPaperReview>> GetReviewsByUserAndPaperIdsAsync(string userId, List<string> paperIds);
//        Task<RevisionPaperReview?> GetRevisionPaperReviewByIdAsync(string revisionPaperReviewId);
//        Task<RevisionPaperReview?> GetRevisionPaperReviewByRevisionPaperAndUserAsync(string revisionPaperId, string userId);
//        Task<List<RevisionPaperReview>> GetRevisionPaperReviewByRevisionPaperIdAsync(string revisionPaperId);
//        Task<int> UpdateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview);
//    }
//    public class RevisionPaperReviewRepository : GenericRepository<RevisionPaperReview>, IRevisionPaperReviewRepository
//    {
//        public RevisionPaperReviewRepository(ConfRadarDbContext context) : base(context)
//        {
//        }

//        public async Task<int> CreateMultipleRevisionPaperReviewsAsync(List<RevisionPaperReview> revisionPaperReviews)
//        {
//            await _context.RevisionPaperReviews.AddRangeAsync(revisionPaperReviews);
//            return await _context.SaveChangesAsync();
//        }

//        public async Task<int> CreateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview)
//        {
//            return await CreateAsync(revisionPaperReview);
//        }

//        public async Task<List<RevisionPaperReview>> GetReviewsByUserAndPaperIdsAsync(string userId, List<string> paperIds)
//        {
//            if (paperIds == null || !paperIds.Any())
//                return new List<RevisionPaperReview>();

//            return await _context.RevisionPaperReviews.AsNoTracking()
//                // Include GlobalStatus để lấy tên trạng thái
//                .Include(r => r.GlobalStatus)
//                // Filter: Của user này VÀ Thuộc RevisionPaper của những PaperId kia
//                .Where(r => r.ReviewerId == userId &&
//                            r.RevisionPaper.Papers.Any(p => paperIds.Contains(p.PaperId)))
//                .ToListAsync();
//        }

//        public async Task<RevisionPaperReview?> GetRevisionPaperReviewByIdAsync(string revisionPaperReviewId)
//        {
//            return await _context.RevisionPaperReviews
//                .FirstOrDefaultAsync(rpr => rpr.RevisionPaperReviewId == revisionPaperReviewId);
//        }

//        public async Task<RevisionPaperReview?> GetRevisionPaperReviewByRevisionPaperAndUserAsync(string revisionPaperId, string userId)
//        {
//            return await _context.RevisionPaperReviews
//                .FirstOrDefaultAsync(rpr => rpr.RevisionPaperId == revisionPaperId && rpr.ReviewerId == userId);
//        }

//        public async Task<List<RevisionPaperReview>> GetRevisionPaperReviewByRevisionPaperIdAsync(string revisionPaperId)
//        {
//            return await _context.RevisionPaperReviews
//                .Include(rpr => rpr.Reviewer)
//                .Include(rpr => rpr.GlobalStatus)
//                .Where(rpr => rpr.RevisionPaperId == revisionPaperId).ToListAsync();
//        }

//        public async Task<int> UpdateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview)
//        {
//            return await UpdateAsync(revisionPaperReview);
//        }
//    }
//}
