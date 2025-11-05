using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Paper
{
    public class CreateCameraReadyRequest
    {
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; }
        [Required(ErrorMessage = "Nộp file là bắt buộc")]

        public IFormFile CameraReadyFile { get; set; }
        [Required(ErrorMessage = "Tiêu đề camera ready là bắt buộc")]

        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả cho camera ready là bắt buộc")]

        public string? Description { get; set; }
    }
}