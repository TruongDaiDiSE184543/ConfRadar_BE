using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRevisionSubmissionFeedbackRepository
    {
        Task<int> CreateMultipleFeedbacksAsync(List<RevisionSubmissionFeedback> feedbacks);
        Task<int> UpdateMultipleFeedbacksAsync(List<RevisionSubmissionFeedback> feedbacks);
        Task<RevisionSubmissionFeedback?> GetFeedbackByIdAsync(string revisionSubmissionFeedbackId);
    }
    public class RevisionSubmissionFeedbackRepository : GenericRepository<RevisionSubmissionFeedback>, IRevisionSubmissionFeedbackRepository
    {
        public RevisionSubmissionFeedbackRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateMultipleFeedbacksAsync(List<RevisionSubmissionFeedback> feedbacks)
        {
            await _context.RevisionSubmissionFeedbacks.AddRangeAsync(feedbacks);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateMultipleFeedbacksAsync(List<RevisionSubmissionFeedback> feedbacks)
        {
            _context.RevisionSubmissionFeedbacks.UpdateRange(feedbacks);
            return await _context.SaveChangesAsync();
        }
        public async Task<RevisionSubmissionFeedback?> GetFeedbackByIdAsync(string revisionSubmissionFeedbackId)
        {
            //return await GetByIdAsync(revisionSubmissionFeedbackId);
            return await _context.RevisionSubmissionFeedbacks.FirstOrDefaultAsync(x => x.RevisionSubmissionFeedbackId == revisionSubmissionFeedbackId);

        }
    }

}
