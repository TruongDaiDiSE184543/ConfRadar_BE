using ConfRadar.Api.Responses;
using ConfRadar.Services;
using ConfRadar.Services.DTOs.ConferenceCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Conference Organizer, Admin")]
    public class ConferenceCategoryController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ConferenceCategoryController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Create a new conference category
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateConferenceCategory([FromBody] CreateConferenceCategoryRequest request)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var category = await _serviceManager.ConferenceCategoryService.CreateConferenceCategoryAsync(request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"tạo danh mục hội nghị mới với tên {request.ConferenceCategoryName} thành công");
            return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Tạo danh mục hội nghị thành công"));

        }

        /// <summary>
        /// Get a conference category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConferenceCategory(string id)
        {

            var category = await _serviceManager.ConferenceCategoryService.GetConferenceCategoryByIdAsync(id);
            return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Conference category retrieved successfully"));

        }

        /// <summary>
        /// Get all conference categories with conference counts
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllConferenceCategories()
        {

            var categories = await _serviceManager.ConferenceCategoryService.GetAllConferenceCategoriesAsync();
            return Ok(ApiResponse<List<ConferenceCategoryListResponse>>.SuccessResponse(categories, "Conference categories retrieved successfully"));



        }

        /// <summary>
        /// Update a conference category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConferenceCategory(string id, [FromBody] UpdateConferenceCategoryRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var category = await _serviceManager.ConferenceCategoryService.UpdateConferenceCategoryAsync(id, request);
            await _serviceManager.AuditLogService.CreateAuditLog(userId, Services.Common.AuditLogActionNameEnum.Conference, $"cập nhật danh mục hội nghị {id} thành công");
            return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Cập nhật danh mục hội nghị thành công"));


        }

        /// <summary>
        /// Delete a conference category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConferenceCategory(string id)
        {
            var result = await _serviceManager.ConferenceCategoryService.DeleteConferenceCategoryAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Xóa danh mục hội nghị thành công"));

        }
    }
}