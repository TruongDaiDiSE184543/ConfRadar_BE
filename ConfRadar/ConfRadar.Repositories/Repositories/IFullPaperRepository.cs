using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IFullPaperRepository
    {
        Task<int> CreateFullPaperAsync(FullPaper fullPaper);
        Task<int> UpdateFullPaperAsync(FullPaper fullPaper);
        Task<bool> DeleteFullPaperAsync(FullPaper fullPaper);
        Task<FullPaper?> GetFullPaperByIdAsync(string fullPaperId);
        Task<List<FullPaper>> GetAllFullPapersAsync();
    }
}