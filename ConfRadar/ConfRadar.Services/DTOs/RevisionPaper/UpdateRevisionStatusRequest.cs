using ConfRadar.Services.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.RevisionPaper
{
    public class UpdateRevisionStatusRequest
    {
        [Required(ErrorMessage = "RevisionPaperId là bắt buộc")]
        public string? RevisionPaperId { get; set; }

        [Required(ErrorMessage = "PaperId là bắt buộc")]
        public string PaperId { get; set; }
        [Required(ErrorMessage = "GlobalStatusEnum là bắt buộc")]
        [EnumDataType(typeof(GlobalStatusEnum), ErrorMessage = "Global status là bắt buộc")]
        public GlobalStatusEnum GlobalStatus { get; set; }
       
    }
}
