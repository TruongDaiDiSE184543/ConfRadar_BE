using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Abstract
{
    public class CreateAbstractRequest
    {
        [Required(ErrorMessage = "Abstract file bắt buộc")]
        public IFormFile AbstractFile { get; set; }
        [Required(ErrorMessage = "Paper id bắt buộc")]
        public string PaperId { get; set; }
        public List<string> CoAuthorId { get; set; }
    }




    public class FullPaperResponse
    {
        public string? ReviewStatus { get; set; }
        public string? FullPaperURL { get; set; }
    }
}
