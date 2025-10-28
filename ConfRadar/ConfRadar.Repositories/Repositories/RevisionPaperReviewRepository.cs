using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
   
    public interface IRevisionPaperReviewRepository
    {
        Task<int> CreateMultipleRevisionPaperReviewsAsync(List<RevisionPaperReview> revisionPaperReviews);
        Task<int> CreateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview);
        Task<RevisionPaperReview?> GetRevisionPaperReviewByIdAsync(string revisionPaperReviewId);
        Task<List<RevisionPaperReview>> GetRevisionPaperReviewByRevisionPaperIdAsync(string revisionPaperId);
        Task<int> UpdateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview);
    }
    public class RevisionPaperReviewRepository : GenericRepository<RevisionPaperReview>, IRevisionPaperReviewRepository
    {
        public RevisionPaperReviewRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateMultipleRevisionPaperReviewsAsync(List<RevisionPaperReview> revisionPaperReviews)
        {
            await _context.RevisionPaperReviews.AddRangeAsync(revisionPaperReviews);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview)
        {
            return await CreateAsync(revisionPaperReview);
        }

        public async Task<RevisionPaperReview?> GetRevisionPaperReviewByIdAsync(string revisionPaperReviewId)
        {
            return await _context.RevisionPaperReviews
                .FirstOrDefaultAsync(rpr => rpr.RevisionPaperReviewId == revisionPaperReviewId);
        }

        public async Task<List<RevisionPaperReview>> GetRevisionPaperReviewByRevisionPaperIdAsync(string revisionPaperId)
        {
            return await _context.RevisionPaperReviews
                .Include(rpr =>rpr.Reviewer)
                .Include(rpr => rpr.GlobalStatus)
                .Where(rpr => rpr.RevisionPaperId == revisionPaperId).ToListAsync();
        }

        public async Task<int> UpdateRevisionPaperReviewAsync(RevisionPaperReview revisionPaperReview)
        {
            return await UpdateAsync(revisionPaperReview);
        }
    }
}
