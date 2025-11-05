// Using statements should be minimal, only what is needed for property types.
namespace ConfRadar.Services.DTOs.Paper
{
    /// <summary>
    /// The main response object containing the complete detail of a paper.
    /// This is the top-level DTO that your API endpoint will return.
    /// </summary>
    public class PaperDetailResponseDtoDetail
    {
        public string PaperId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public PaperPhaseDtoDetail? CurrentPhase { get; set; }
        public AbstractDtoDetail? Abstract { get; set; }
        public FullPaperDtoDetail? FullPaper { get; set; }
        public RevisionPaperDtoDetail? RevisionPaper { get; set; }
        public CameraReadyDtoDetail? CameraReady { get; set; }
        public ResearchPhaseDtoDetail? ResearchPhase { get; set; }
        public List<RevisionDeadlineDetail>? revisionDeadline { get; set; }
    }

    public class ResearchPhaseDtoDetail
    {
        public string ResearchConferencePhaseId { get; set; } = null!;

        public string? ConferenceId { get; set; }

        public DateOnly? RegistrationStartDate { get; set; }

        public DateOnly? RegistrationEndDate { get; set; }

        public DateOnly? FullPaperStartDate { get; set; }

        public DateOnly? FullPaperEndDate { get; set; }

        public DateOnly? ReviewStartDate { get; set; }

        public DateOnly? ReviewEndDate { get; set; }

        public DateOnly? ReviseStartDate { get; set; }

        public DateOnly? ReviseEndDate { get; set; }

        public DateOnly? CameraReadyStartDate { get; set; }

        public DateOnly? CameraReadyEndDate { get; set; }
    }


    /// <summary>
    /// Represents the current phase of the paper submission process.
    /// </summary>
    public class PaperPhaseDtoDetail
    {
        public string PaperPhaseId { get; set; }
        public string PhaseName { get; set; }
    }

    /// <summary>
    /// Represents the details of the submitted abstract.
    /// </summary>
    public class AbstractDtoDetail
    {
        public string AbstractId { get; set; }
        public string FileUrl { get; set; } // Assuming a 'Content' property exists on the entity
        public string Status { get; set; } // The name of the GlobalStatus, e.g., "Pending", "Approved"
    }

    /// <summary>
    /// Represents the details of the submitted full paper.
    /// </summary>
    public class FullPaperDtoDetail
    {
        public string FullPaperId { get; set; }
        public string FileUrl { get; set; }
        public string ReviewStatus { get; set; } // The name of the ReviewStatus, e.g., "Under Review"
        public string RootPaperId { get; set; }
    }

    /// <summary>
    /// Represents the details of the camera-ready version of the paper.
    /// </summary>
    public class CameraReadyDtoDetail
    {
        public string CameraReadyId { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; } // The name of the GlobalStatus
        public string RootPaperId { get; set; }
    }

    /// <summary>
    /// Contains all information related to the revision process of a paper.
    /// </summary>
    public class RevisionPaperDtoDetail
    {
        public string RevisionPaperId { get; set; }
        public int? RevisionRound { get; set; }
        public string OverallStatus { get; set; } // The name of the GlobalStatus for the revision
        public List<RevisionSubmissionDtoDetail> Submissions { get; set; } = new List<RevisionSubmissionDtoDetail>();

        public List<RevisionReviewDtoDetail> Reviews { get; set; } = new List<RevisionReviewDtoDetail>();
    }

    /// <summary>
    /// Represents a single submission within a revision round.
    /// </summary>
    public class RevisionSubmissionDtoDetail
    {
        public string SubmissionId { get; set; }
        public string FileUrl { get; set; }
        public List<FeedbackDtoDetail> Feedbacks { get; set; } = new List<FeedbackDtoDetail>();
    }

    /// <summary>
    /// Represents a review provided for a revision.
    /// </summary>
    public class RevisionReviewDtoDetail
    {
        public string ReviewId { get; set; }
        public string Note { get; set; }
        public string FeedBackToAuthor { get; set; } // e.g., "Changes Required", "Approved"
        public string FeedbackMaterialURL { get; set; }
        public DateTime ReviewedAt { get; set; }
    }

    /// <summary>
    /// Represents feedback on a specific revision submission.
    /// </summary>
    public class FeedbackDtoDetail
    {
        public string FeedbackId { get; set; }
        public string FeedBack { get; set; }
        public string Response { get; set; } // The name of the user who gave the feedback
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class RevisionDeadlineDetail
    {
        public string RevisionRoundDeadlineId { get; set; } = null!;

        public DateOnly? StartSubmissionDate { get; set; }
        public DateOnly? EndSubmissionDate { get; set; }

        public int? RoundNumber { get; set; }

        public string? ResearchConferencePhaseId { get; set; }

    }


    public class PaperDetailResponseDto
    {
        public string PaperId { get; set; }
        public PaperPhaseDto? CurrentPhase { get; set; }
        public AbstractDto? Abstract { get; set; }
        public FullPaperDto? FullPaper { get; set; }
        public RevisionPaperDto? RevisionPaper { get; set; }
        public CameraReadyDto? CameraReady { get; set; }
    }

    /// <summary>
    /// Represents the current phase of the paper submission process.
    /// </summary>
    public class PaperPhaseDto
    {
        public string PaperPhaseId { get; set; }
        public string PhaseName { get; set; }
    }

    /// <summary>
    /// Represents the details of the submitted abstract.
    /// </summary>
    public class AbstractDto
    {
        public string AbstractId { get; set; }
        public string FileUrl { get; set; } // Assuming a 'Content' property exists on the entity
        public string Status { get; set; } // The name of the GlobalStatus, e.g., "Pending", "Approved"
    }

    /// <summary>
    /// Represents the details of the submitted full paper.
    /// </summary>
    public class FullPaperDto
    {
        public string FullPaperId { get; set; }
        public string FileUrl { get; set; }
        public string ReviewStatus { get; set; } // The name of the ReviewStatus, e.g., "Under Review"
    }

    /// <summary>
    /// Represents the details of the camera-ready version of the paper.
    /// </summary>
    public class CameraReadyDto
    {
        public string CameraReadyId { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; } // The name of the GlobalStatus
    }

    /// <summary>
    /// Contains all information related to the revision process of a paper.
    /// </summary>
    public class RevisionPaperDto
    {
        public string RevisionPaperId { get; set; }
        public int? RevisionRound { get; set; }
        public string OverallStatus { get; set; } // The name of the GlobalStatus for the revision
        public List<RevisionSubmissionDto> Submissions { get; set; } = new List<RevisionSubmissionDto>();
        public List<RevisionReviewDto> Reviews { get; set; } = new List<RevisionReviewDto>();
    }

    /// <summary>
    /// Represents a single submission within a revision round.
    /// </summary>
    public class RevisionSubmissionDto
    {
        public string SubmissionId { get; set; }
        public string FileUrl { get; set; }
        public RevisionDeadline revisionDeadline { get; set; }
        public List<FeedbackDto> Feedbacks { get; set; } = new List<FeedbackDto>();
    }

    /// <summary>
    /// Represents a review provided for a revision.
    /// </summary>
    public class RevisionReviewDto
    {
        public string ReviewId { get; set; }
        public string Note { get; set; }
        public string FeedBackToAuthor { get; set; } // e.g., "Changes Required", "Approved"
        public string FeedbackMaterialURL { get; set; }
        public DateTime ReviewedAt { get; set; }
    }

    /// <summary>
    /// Represents feedback on a specific revision submission.
    /// </summary>
    public class FeedbackDto
    {
        public string FeedbackId { get; set; }
        public string FeedBack { get; set; }
        public string Response { get; set; } // The name of the user who gave the feedback
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class RevisionDeadline
    {
        public int? RoundNumher { get; set; } // e.g., "Revision Round 1"
        public DateOnly? Deadline { get; set; }
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






