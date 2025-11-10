using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Paper
{
    public class UpdateAbstractRequest
    {
        public IFormFile? AbstractFile { get; set; }
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? CoAuthorId { get; set; }
    }
}
