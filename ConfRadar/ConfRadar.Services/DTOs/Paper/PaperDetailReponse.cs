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

    public class PaperDetailResponseDTO
    {
        public string PaperId { get; set; }
        public PaperPhaseResponseDTO currentPhase { get; set; }
        public AbstractResponseDTO? Abstract { get; set; }
        public FullPaperResponseDTO? FullPaper { get; set; }
        public RevisionPaperResponseDTO? RevisionPaper { get; set; }
        public CameraReadyResponseDTO? CameraReady { get; set; }
    }

    public class PaperPhaseResponseDTO
    {
        public string PaperPhaseId { get; set; } = null!;

        public string? PhaseName { get; set; }

    }
    public class AbstractResponseDTO
    {
        public string AbstractId { get; set; } = null!;

        public string? GlobalStatusId { get; set; }
        public string GlobalStatusName { get; set; }
        public string? AbstractUrl { get; set; }

    }
    public class FullPaperResponseDTO
    {
        public string FullPaperId { get; set; } = null!;

        public string? ReviewStatusId { get; set; }
        public string? ReviewStatusName { get; set; }

        public string? FullPaperUrl { get; set; }


    }
    public class RevisionPaperResponseDTO
    {
        public string RevisionPaperId { get; set; } = null!;

        public int? RevisionRound { get; set; }

        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }


    }
    public class CameraReadyResponseDTO
    {
        public string CameraReadyId { get; set; } = null!;

        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? CameraReadyUrl { get; set; }

    }



}
