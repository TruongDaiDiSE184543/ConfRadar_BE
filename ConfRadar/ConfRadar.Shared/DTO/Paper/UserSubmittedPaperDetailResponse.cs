namespace ConfRadar.Shared.DTO.Paper
{
    public class UserSubmittedPaperDetailResponse
    {
        public string PaperId { get; set; } = null!;
        public string? AbstractId { get; set; }
        public string? FullPaperId { get; set; }
        public string? RevisionPaperId { get; set; }
        public string? CameraReadyId { get; set; }




        public string? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceDescription { get; set; }
        public DateOnly? ConferenceStartDate { get; set; }
        public DateOnly? ConferenceEndDate { get; set; }
        public int? ConferenceTotalSlot { get; set; }
        public int? ConferenceAvailableSlot { get; set; }
        public string? Address { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime? ConferenceCreatedAt { get; set; }
        public DateOnly? TicketSaleStart { get; set; }
        public DateOnly? TicketSaleEnd { get; set; }
        public bool? IsInternalHosted { get; set; }
        public bool? IsResearchConference { get; set; }

        public string? CityId { get; set; }
        public string? CityName { get; set; }



        public string? ConferenceCreatedBy { get; set; }
        public string? ConferenceCreatedByEmail { get; set; }
        public string? ConferenceCreatedByFullName { get; set; }
        public string? ConferenceCreatedByAvatarUrl { get; set; }






        public string? ConferenceCategoryId { get; set; }
        public string? ConferenceStatusId { get; set; }








        public string? PaperPhaseId { get; set; }
        public string? PhaseName { get; set; }



        public string? ResearchConferencePhaseId { get; set; }
        public string? TicketId { get; set; }
        public DateTime? PaperCreatedAt { get; set; }
        public string? PaperTitle { get; set; }
        public string? PaperDescription { get; set; }

        public UserSubmittedAbstract? Abstract { get; set; }
        public UserSubmittedFullPaper? FullPaper { get; set; }
        public UserSubmittedRevisionPaper? RevisionPaper { get; set; }
        public UserSubmittedCameraReady? CameraReady { get; set; }

    }
    public class UserSubmittedAbstract
    {
        public string AbstractId { get; set; } = null!;
        public string? AbstractUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewAt { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }

    }
    public class UserSubmittedFullPaper
    {
        public string FullPaperId { get; set; } = null!;
        public string? ReviewStatusId { get; set; }
        public string? ReviewStatusName { get; set; }
        public string? FullPaperUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewAt { get; set; }
    }
    public class UserSubmittedRevisionPaper
    {
        public string RevisionPaperId { get; set; } = null!;
        public int? RevisionRound { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewAt { get; set; }
        public string? RevisionRoundDeadlineId { get; set; }
        public DateOnly? RevisionRoundDeadlineStartSubmissionDate { get; set; }
        public DateOnly? RevisionRoundDeadlineEndSubmissionDate { get; set; }
        public int? RevisionRoundDeadlineRoundNumber { get; set; }

    }
    public class UserSubmittedCameraReady
    {
        public string CameraReadyId { get; set; } = null!;

        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }


        public string? CameraReadyUrl { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ReviewAt { get; set; }


    }


}
