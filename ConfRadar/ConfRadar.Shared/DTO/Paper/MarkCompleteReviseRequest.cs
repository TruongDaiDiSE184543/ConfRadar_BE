using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.Paper
{
    public class MarkCompleteReviseRequest
    {
        [Required(ErrorMessage ="Mã revision round deadline là bắt buộc")]
        public string RevisionRoundDeadlineId { get; set; }
        [Required(ErrorMessage = "Mã revision paper là bắt buộc")]
        public string RevisionPaperId { get; set; } = null!;
        [Required(ErrorMessage = "Mã bài báo là bắt buộc")]
        public string PaperId { get; set; } = null!;

    }
}
