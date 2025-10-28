using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.RankingCategory;
using ConfRadar.Services.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConferenceRankingController :ControllerBase
    {
        private readonly IServiceManager _services;
        public ConferenceRankingController (IServiceManager services) => _services = services;
        [HttpGet("Get-all-ranking-category")]
        public async Task<IActionResult> GetAllRankingCategory()
        {
            var result = await _services.RankingCategoryService.GetAllRankingCategory();
            List<RankingCategoryResponseDTO> rankingcategoryresponseDTOList = result.Select(rc => new RankingCategoryResponseDTO
            {
                rankId = rc.RankingCategoryId,
                description = rc.RankDescription,
                rankName = rc.RankName
            }).ToList();
            return Ok(ApiResponse<List<RankingCategoryResponseDTO>>.SuccessResponse(rankingcategoryresponseDTOList));
        }
    }
}
