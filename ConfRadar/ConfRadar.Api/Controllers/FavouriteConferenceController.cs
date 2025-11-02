using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using ConfRadar.Shared.DTO.FavouriteConference;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouriteConferenceController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public FavouriteConferenceController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [Authorize]
        [HttpGet("list-own-favourite-conferences")]
        public async Task<ActionResult<List<FavouriteConferenceDetailResponse>>> GetOwnFavourites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var favourites = await _serviceManager.FavoriteConferenceService.GetFavouritesByUserIdAsync(userId);
            return Ok(ApiResponse<List<FavouriteConferenceDetailResponse>>.SuccessResponse(favourites, "Danh sách yêu thích của bạn"));
        }
        [Authorize]
        [HttpPost("add-to-favourite")]
        public async Task<ActionResult<AddedFavouriteConfereceResponse>> AddFavourite([FromBody] FavouriteConferenceRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.FavoriteConferenceService.AddFavouriteAsync(userId,request.ConferenceId);
            return Ok(ApiResponse<AddedFavouriteConfereceResponse>.SuccessResponse(result, "Đã thêm sự kiện vào danh mục yêu thích"));

        }
        [Authorize]
        [HttpDelete("delete-from-favourite")]
        public async Task<ActionResult<DeletedFavouriteConfereceResponse>> DeleteFavourite([FromBody] FavouriteConferenceRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _serviceManager.FavoriteConferenceService.DeleteFavouriteAsync(userId, request.ConferenceId);
            return Ok(ApiResponse<DeletedFavouriteConfereceResponse>.SuccessResponse(result, "Đã xóa sự kiện khỏi danh mục yêu thích"));
        }
    }
}
