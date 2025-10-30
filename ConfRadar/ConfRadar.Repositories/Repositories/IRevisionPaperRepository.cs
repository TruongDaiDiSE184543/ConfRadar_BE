using ConfRadar.Repositories.Models;

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