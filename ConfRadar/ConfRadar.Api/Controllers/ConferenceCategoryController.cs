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
            try
            {
                var category = await _serviceManager.ConferenceCategoryService.CreateConferenceCategoryAsync(request);
                return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Conference category created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get a conference category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConferenceCategory(string id)
        {
            try
            {
                var category = await _serviceManager.ConferenceCategoryService.GetConferenceCategoryByIdAsync(id);
                return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Conference category retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get all conference categories with conference counts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllConferenceCategories()
        {
            try
            {
                var categories = await _serviceManager.ConferenceCategoryService.GetAllConferenceCategoriesAsync();
                return Ok(ApiResponse<List<ConferenceCategoryListResponse>>.SuccessResponse(categories, "Conference categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update a conference category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConferenceCategory(string id, [FromBody] UpdateConferenceCategoryRequest request)
        {
            try
            {
                var category = await _serviceManager.ConferenceCategoryService.UpdateConferenceCategoryAsync(id, request);
                return Ok(ApiResponse<ConferenceCategoryResponse>.SuccessResponse(category, "Conference category updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete a conference category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConferenceCategory(string id)
        {
            try
            {
                var result = await _serviceManager.ConferenceCategoryService.DeleteConferenceCategoryAsync(id);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Conference category deleted successfully"));
                }
                return NotFound(ApiResponse<object>.FailResponse("Conference category not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }
}