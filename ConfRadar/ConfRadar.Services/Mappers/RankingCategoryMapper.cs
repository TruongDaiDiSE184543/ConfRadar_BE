using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.RankingCategory;

namespace ConfRadar.Services.Mappers
{
    public static class RankingCategoryMapper
    {
        public static RankingCategoryResponseDTO toResponse(this RankingCategory model)
        {
            return new RankingCategoryResponseDTO
            {
                rankId = model.RankingCategoryId,
                description = model.RankDescription
            };
        }
    }
}
