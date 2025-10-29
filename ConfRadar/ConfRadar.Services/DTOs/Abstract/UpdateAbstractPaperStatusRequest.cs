using ConfRadar.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Abstract
{
    public class UpdateAbstractPaperStatusRequest
    {
        [Required(ErrorMessage ="Paper id thì bắt buộc")]
        public string PaperId { get; set; }
        [Required(ErrorMessage = "Abstract id thì bắt buộc")]
        public string AbstractId { get; set; }
        [Required(ErrorMessage ="Global status là bắt buộc để quyết định trạng thái")]
        [EnumDataType(typeof(GlobalStatusEnum), ErrorMessage = "Global status là bắt buộc")]
        public GlobalStatusEnum GlobalStatus{ get; set; }
    }
}
