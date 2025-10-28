using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperReviewerRepository
    {
        Task<int> CreatePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<int> UpdatePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<bool> DeletePaperReviewerAsync(PaperReviewer paperReviewer);
        Task<PaperReviewer?> GetPaperReviewerByIdAsync(string? userId, string? paperId);
        Task<List<PaperReviewer>> GetAllPaperReviewersAsync();
        Task<List<PaperReviewer>> GetPaperReviewersByPaperIdAsync(string paperId);
        Task<List<PaperReviewer>> GetPaperReviewersByUserIdAsync(string userId);
        Task<List<PaperReviewer>> GetHeadReviewersByPaperIdAsync(string paperId);
    }
}