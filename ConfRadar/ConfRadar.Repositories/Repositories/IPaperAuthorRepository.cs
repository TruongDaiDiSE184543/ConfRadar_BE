using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperAuthorRepository
    {
        Task<int> CreatePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<int> UpdatePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<bool> DeletePaperAuthorAsync(PaperAuthor paperAuthor);
        Task<PaperAuthor?> GetPaperAuthorByIdAsync(string? userId, string? paperId);
        Task<List<PaperAuthor>> GetAllPaperAuthorsAsync();
        Task<List<PaperAuthor>> GetPaperAuthorsByPaperIdAsync(string paperId);
        Task<List<PaperAuthor>> GetPaperAuthorsByUserIdAsync(string userId);
        Task<int> CreateMutiplePaperAuthorAsync(List<PaperAuthor> paperAuthor);
    }
}