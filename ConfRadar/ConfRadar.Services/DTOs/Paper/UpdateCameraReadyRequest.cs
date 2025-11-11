using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Paper
{
    public class UpdateCameraReadyRequest
    {
        [Required(ErrorMessage = "Mã camera ready là bắt buộc")]
        public string CameraReadyId { get; set; }
        public IFormFile? CameraReadyFile { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}