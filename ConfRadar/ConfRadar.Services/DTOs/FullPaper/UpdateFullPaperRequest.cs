using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.FullPaper
{
    public class CreateFullPaperRequest
    {
        [Required(ErrorMessage = "Full paper file là bắt buộc")]
        public IFormFile FullPaperFile { get; set; }
        [Required(ErrorMessage = "Paper id là bắt buộc")]
        public string PaperId { get; set; }
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
