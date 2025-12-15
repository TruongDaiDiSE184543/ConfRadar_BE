using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Abstract
{
    public class CreateAbstractRequest
    {
        [Required(ErrorMessage = "Abstract file bắt buộc")]
        public IFormFile AbstractFile { get; set; }

        [Required(ErrorMessage = "Tiêu đề cho abstract là bắt buộc")]

        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả cho abstract là bắt buộc")]

        public string? Description { get; set; }
        [Required(ErrorMessage = "Mã hội nghị là bắt buộc")]

        public string ConferenceId { get; set; }

        public List<string>? CoAuthorId { get; set; }

        [Required(ErrorMessage = "Mã phiên là bắt buộc")]

        public string ConferenceSessionId { get; set; }
    }






    public class FullPaperResponse
    {
        public string? ReviewStatus { get; set; }
        public string? FullPaperURL { get; set; }
    }


    public class UpdatePaperRequest
    {
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]

        public string PaperId { get; set; }


        [Required(ErrorMessage = "Tiêu đề cho bài báo là bắt buộc")]

        public string Title { get; set; }


        [Required(ErrorMessage = "Mô tả cho bài báo là bắt buộc")]

        public string? Description { get; set; }
    }

}
