using ConfRadar.Repositories.Models;

namespace ConfRadar.Repositories.Repositories
{
    public interface IFullPaperReviewRepository
    {
        Task<int> CreateFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<int> UpdateFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<bool> DeleteFullPaperReviewAsync(FullPaperReview fullPaperReview);
        Task<FullPaperReview?> GetFullPaperReviewByIdAsync(string fullPaperReviewId);
        Task<List<FullPaperReview>> GetAllFullPaperReviewsAsync();
        Task<List<FullPaperReview>> GetFullPaperReviewsByFullPaperIdAsync(string fullPaperId);
        Task<List<FullPaperReview>> GetFullPaperReviewsByReviewerIdAsync(string reviewerId);
        Task<FullPaperReview?> GetFullPaperReviewByFullPaperIdAndReviewerIdAsync(string fullPaperId, string reviewerId);
    }
}