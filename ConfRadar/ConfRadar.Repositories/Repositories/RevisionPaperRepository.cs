using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IRevisionPaperRepository
    {
        Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<RevisionPaper?> GetRevisionPaperByIdAsync(string revisionPaperId);
        Task<List<RevisionPaper>> GetAllRevisionPapersAsync();
        Task<RevisionPaper> GetDetailRevisionPaper(string revisionPaperId);
    }
}
public class RevisionPaperRepository : GenericRepository<RevisionPaper>, IRevisionPaperRepository
{
    public RevisionPaperRepository(ConfRadarDbContext context) : base(context) { }

    public async Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper)
    {
        return await CreateAsync(revisionPaper);
    }

    public async Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper)
    {
        return await UpdateAsync(revisionPaper);
    }

    public async Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper)
    {
        return await RemoveAsync(revisionPaper);
    }

    public async Task<RevisionPaper?> GetRevisionPaperByIdAsync(string revisionPaperId)
    {
        return await _context.RevisionPapers
            .AsSplitQuery()
            .Include(rp => rp.RevisionRoundDeadline)
            .Include(rp => rp.RevisionPaperSubmissions)
                .ThenInclude(rps => rps.RevisionDeadlineRound)
            .Include(rp => rp.RevisionPaperSubmissions)
                .ThenInclude(rps => rps.RevisionSubmissionFeedbacks)
            .FirstOrDefaultAsync(x => x.RevisionPaperId == revisionPaperId);
    }

    public async Task<List<RevisionPaper>> GetAllRevisionPapersAsync()
    {
        return await GetAllAsync();
    }

    public async Task<RevisionPaper?> GetDetailRevisionPaper(string revisionPaperId)
    {
        return await _context.RevisionPapers
            .Include(rvp => rvp.RevisionPaperSubmissions)
                .ThenInclude(rps => rps.RevisionDeadlineRound)
            .Include(rvp => rvp.RevisionPaperSubmissions)
                .ThenInclude(rps => rps.RevisionSubmissionFeedbacks)
            .Include(rvp => rvp.RevisionPaperReviews)
            .Include(rp=>rp.GlobalStatus)
            .FirstOrDefaultAsync(rvp => rvp.RevisionPaperId == revisionPaperId);
    }
}
