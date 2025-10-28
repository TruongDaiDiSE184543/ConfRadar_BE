using ConfRadar.Api.Responses;
using ConfRadar.Repositories.Models;
using ConfRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfRadar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public CityController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
        [HttpGet("cities")]
        public async Task<IActionResult> GetAllCities()
        {
            var result = await _serviceManager.CityService.GetAllCitiesAsync();
            return Ok(ApiResponse<List<City>>.SuccessResponse(result, "thông tin city"));
        }
    }
}
