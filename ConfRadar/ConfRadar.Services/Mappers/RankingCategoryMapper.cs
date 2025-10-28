using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.RankingCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class RankingCategoryMapper
    {
        public static RankingCategoryResponseDTO toResponse(this  RankingCategory model)
        {
            return new RankingCategoryResponseDTO
            {
                rankId = model.RankingCategoryId,
                description = model.RankDescription
            };
        }
    }
}
