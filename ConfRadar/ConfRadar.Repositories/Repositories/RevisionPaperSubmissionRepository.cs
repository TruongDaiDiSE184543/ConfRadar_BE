using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRevisionPaperSubmissionRepository
    {
        Task<int> CreateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission);
        Task<int> CreateMultipleRevisionPaperSubmissionsAsync(List<RevisionPaperSubmission> submissions);
        Task<RevisionPaperSubmission?> GetRevisionPaperSubmissionByIdAsync(string revisionPaperSubmissionId);
        Task<int> UpdateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission);
    }
    public class RevisionPaperSubmissionRepository : GenericRepository<RevisionPaperSubmission>, IRevisionPaperSubmissionRepository
    {
        public RevisionPaperSubmissionRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission)
        {
            return await CreateAsync(submission);
        }

        public async Task<int> CreateMultipleRevisionPaperSubmissionsAsync(List<RevisionPaperSubmission> submissions)
        {
            await _context.RevisionPaperSubmissions.AddRangeAsync(submissions);
            return await _context.SaveChangesAsync();
        }

        public async Task<RevisionPaperSubmission?> GetRevisionPaperSubmissionByIdAsync(string revisionPaperSubmissionId)
        {
            return await _context.RevisionPaperSubmissions
                .Include(rps => rps.RevisionPaper)
                .FirstOrDefaultAsync(rps => rps.RevisionPaperSubmissionId == revisionPaperSubmissionId);
        }
        public async Task<int> UpdateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission)
        {
            return await UpdateAsync(submission);
        }
    }

}
