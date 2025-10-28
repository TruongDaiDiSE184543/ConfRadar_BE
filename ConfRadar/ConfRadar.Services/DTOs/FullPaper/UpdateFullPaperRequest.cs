using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.FullPaper
{
    public class UpdateFullPaperRequest
    {
        [Required(ErrorMessage = "Paper id là bắt buộc")]
        public string PaperId { get; set; } = null!;
        [Required(ErrorMessage = "Full Paper id là bắt buộc")]
        public string FullPaperId { get; set; }
        [Required(ErrorMessage = "Gửi file là bắt buộc")]
        public IFormFile? FullPaperFile { get; set; }

    }
    public class UpdateFullPaperStatusRequest
    {
        [Required(ErrorMessage = "Paper id là bắt buộc")]
        public string PaperId { get; set; } = null!;
        [Required(ErrorMessage = "Full Paper id là bắt buộc")]
        public string FullPaperId { get; set; }
        [EnumDataType(typeof(ReviewStatusEnum), ErrorMessage = "Review status là bắt buộc")]
        public ReviewStatusEnum? ReviewStatus { get; set; }

    }
}
