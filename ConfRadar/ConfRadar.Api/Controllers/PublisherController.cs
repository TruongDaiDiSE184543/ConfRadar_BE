//using ConfRadar.Api.Responses;
//using ConfRadar.Services;
//using ConfRadar.Services.DTOs.ConferenceStep;
//using Microsoft.AspNetCore.Mvc;

//namespace ConfRadar.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PublisherController : ControllerBase
//    {
//        private readonly IServiceManager _serviceManager;

//        public PublisherController(IServiceManager serviceManager)
//        {
//            _serviceManager = serviceManager;
//        }

//        [HttpGet] // Sửa: Dùng route gốc cho Get All
//        public async Task<IActionResult> GetAllPublishers()
//        {
//            var result = await _serviceManager.PublisherService.GetAllPublishersAsync();
//            return Ok(ApiResponse<List<PublisherResponse>>.SuccessResponse(result, "Danh sách nhà xuất bản được truy xuất thành công"));
//        }

//        [HttpGet("{id}")] // Sửa: Dùng route parameter cho Get By Id
//        public async Task<IActionResult> GetPublisher(string id)
//        {
//            var result = await _serviceManager.PublisherService.GetPublisherByIdAsync(id);
//            return Ok(ApiResponse<PublisherResponse>.SuccessResponse(result, "Nhà xuất bản được truy xuất thành công"));
//        }

//        [HttpPost] // Sửa: Dùng route gốc cho Create
//        public async Task<IActionResult> CreatePublisher([FromBody] PublisherRequest request)
//        {
//            var result = await _serviceManager.PublisherService.CreatePublisherAsync(request);
//            // Sửa: Trả về CreatedAtAction để tuân thủ REST standard
//            return CreatedAtAction(nameof(GetPublisher), new { id = result }, ApiResponse<string>.SuccessResponse(result, "Nhà xuất bản được tạo thành công"));
//        }

//        [HttpPut("{id}")] // Sửa: Dùng route parameter cho Update
//        public async Task<IActionResult> UpdatePublisher(string id, [FromBody] PublisherRequest request)
//        {
//            await _serviceManager.PublisherService.UpdatePublisherAsync(id, request);
//            // Sửa: Trả về NoContent() hoặc Ok() cho Update
//            return Ok(ApiResponse<string>.SuccessResponse("Cập nhật nhà xuất bản thành công."));
//        }

//        // === PHIÊN BẢN HOÀN CHỈNH CỦA DELETE ENDPOINT ===
//        [HttpDelete("{id}")] // Sửa: Dùng route parameter cho Delete
//        public async Task<IActionResult> DeletePublisher(string id)
//        {
//            var result = await _serviceManager.PublisherService.DeletePublisherAsync(id);

//            // Logic service đã xử lý việc ném exception nếu có lỗi.
//            // Nếu code chạy đến đây, có nghĩa là đã xóa thành công (hoặc không tìm thấy để xóa).
//            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa nhà xuất bản thành công."));
//        }
//    }
//}