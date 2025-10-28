using Microsoft.AspNetCore.Http;

namespace ConfRadar.Services.DTOs.Paper
{
    public class CreateCameraReadyRequest
    {
        public string PaperId { get; set; }
        public IFormFile CameraReadyFile { get; set; }
    }
}