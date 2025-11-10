using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Shared.DTO.Paper
{
    public class UpdateFullPaperRequest
    {
        public IFormFile? FullPaperFile { get; set; }
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; }

        public string? Title { get; set; }


        public string? Description { get; set; }
    }
}
