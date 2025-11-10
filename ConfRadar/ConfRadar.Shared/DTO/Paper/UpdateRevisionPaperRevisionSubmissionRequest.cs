using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Paper
{
    public class UpdateRevisionPaperRevisionSubmissionRequest
    {
        public IFormFile? RevisionPaperFile { get; set; }


        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; }
        [Required(ErrorMessage = "Revision Paper submission id là bất buộc")]
        public string RevisionPaperSubmissionId { get; set; } = null!;

        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
