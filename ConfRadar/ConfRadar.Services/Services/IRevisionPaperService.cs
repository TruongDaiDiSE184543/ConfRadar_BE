using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IRevisionPaperService
    {
        Task<int> CreateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<int> UpdateRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<bool> DeleteRevisionPaperAsync(RevisionPaper revisionPaper);
        Task<RevisionPaper?> GetRevisionPaperByIdAsync(string revisionPaperId);
        Task<List<RevisionPaper>> GetAllRevisionPapersAsync();
    }
}