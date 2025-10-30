// Using statements should be minimal, only what is needed for property types.
using System;
using System.Collections.Generic;

namespace ConfRadar.Services.DTOs.Paper
{
    /// <summary>
    /// The main response object containing the complete detail of a paper.
    /// This is the top-level DTO that your API endpoint will return.
    /// </summary>
    public class PaperDetailResponseDtoDetail
    {
        public string PaperId { get; set; }
        public PaperPhaseDtoDetail? CurrentPhase { get; set; }
        public AbstractDtoDetail? Abstract { get; set; }
        public FullPaperDtoDetail? FullPaper { get; set; }
        public RevisionPaperDtoDetail? RevisionPaper { get; set; }
        public CameraReadyDtoDetail? CameraReady { get; set; }
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
    }

    /// <summary>
    /// Represents the details of the camera-ready version of the paper.
    /// </summary>
    public class CameraReadyDtoDetail
    {
        public string CameraReadyId { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; } // The name of the GlobalStatus
    }

    /// <summary>
    /// Contains all information related to the revision process of a paper.
    /// </summary>
    public class RevisionPaperDtoDetail
    {
        public string RevisionPaperId { get; set; }
        public int? RevisionRound {  get; set; }
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
        public RevisionDeadlineDetail revisionDeadline { get; set; }
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
        public int Order {  get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class RevisionDeadlineDetail
    {
        public int? RoundNumher { get; set; } // e.g., "Revision Round 1"
        public DateOnly? Deadline { get; set; }
    }


    public class PaperDetailResponseDTO
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
        public int? RevisionRound {  get; set; }
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
        public int Order {  get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class RevisionDeadline
    {
        public int? RoundNumher { get; set; } // e.g., "Revision Round 1"
        public DateOnly? Deadline { get; set; }
    }
}

