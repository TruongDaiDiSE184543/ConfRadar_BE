using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Paper
{
    public class PaperDetailReponse
    {
        public string PaperId { get; set; }
        public PaperPhase currentPhase {get; set;}
        public ConfRadar.Repositories.Models.Abstract? Abstract { get; set; } 
        public ConfRadar.Repositories.Models.FullPaper? FullPaper { get; set; } 
        public ConfRadar.Repositories.Models.RevisionPaper? RevisionPaper { get; set; } 
        public ConfRadar.Repositories.Models.CameraReady? CameraReady { get; set; } 
    }
}
