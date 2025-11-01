using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IRankingCategoryService
    {
        Task<List<RankingCategory>> GetAllRankingCategory();
    }
    public class RankingCategoryService : IRankingCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RankingCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RankingCategory>> GetAllRankingCategory()
        {
            return await _unitOfWork.RankingCategoryRepository.GetAllRankingCategoryAsync();
        }
    }
}
