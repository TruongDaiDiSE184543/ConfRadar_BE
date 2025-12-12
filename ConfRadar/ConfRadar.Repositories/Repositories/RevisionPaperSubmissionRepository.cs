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
        Task<RevisionPaperSubmission?> GetRevisionPaperSubmissionByRevisionPaperIdAndDeadlineId(string revisionPaperId, string deadlineId);
        Task<int> UpdateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission);
        Task<List<RevisionPaperSubmission>> GetRevisionPaperSubmissionByDeadlineId(string deadlineId);
        Task<List<RevisionPaperSubmission>> GetRevisionPaperSubmissionByRevisionId(string revisionId);
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
                .Include(rps => rps.RevisionDeadlineRound)
                .FirstOrDefaultAsync(rps => rps.RevisionPaperSubmissionId == revisionPaperSubmissionId);
        }
        public async Task<int> UpdateRevisionPaperSubmissionAsync(RevisionPaperSubmission submission)
        {
            return await UpdateAsync(submission);
        }

        public async Task<RevisionPaperSubmission?> GetRevisionPaperSubmissionByRevisionPaperIdAndDeadlineId(string revisionPaperId, string deadlineId)
        {
            return await _context.RevisionPaperSubmissions
                .Include(rp => rp.RevisionDeadlineRound)
                .FirstOrDefaultAsync(x => x.RevisionPaperId == revisionPaperId && x.RevisionDeadlineRoundId == deadlineId);
        }

        public async Task<List<RevisionPaperSubmission>> GetRevisionPaperSubmissionByDeadlineId(string deadlineId)
        {
            return await _context.RevisionPaperSubmissions.Where(rps => rps.RevisionDeadlineRoundId == deadlineId).ToListAsync();
        }

        public async Task<List<RevisionPaperSubmission>> GetRevisionPaperSubmissionByRevisionId(string revisionId)
        {
            return await _context.RevisionPaperSubmissions
                .Include(rps=>rps.RevisionDeadlineRound)
                .Where(rps => rps.RevisionPaperId == revisionId).ToListAsync();
        }
    }

}
