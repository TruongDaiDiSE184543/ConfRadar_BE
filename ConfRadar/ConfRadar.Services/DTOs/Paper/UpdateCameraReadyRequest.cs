using Microsoft.AspNetCore.Http;

namespace ConfRadar.Services.DTOs.Paper
{
    public class UpdateCameraReadyRequest
    {
        public string CameraReadyId { get; set; }
        public IFormFile CameraReadyFile { get; set; }
    }
}