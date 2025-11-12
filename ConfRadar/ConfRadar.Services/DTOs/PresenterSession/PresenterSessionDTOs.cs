using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.PresenterSession
{
    public class PresenterSessionResponse
    {
        public string? ConferenceSessionId { get; set; }
        public string? PaperId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? PaperTitle { get; set; }
        public string? SessionId { get; set; }
        public string? PresenterName { get; set; }
        public string? UserId { get; set; }
    }

    public class PresenterChangeRequest
    {
        public string? PresenterChangeRequestId { get; set; }
        public string? TicketId { get; set; }
        public string? RequestedById { get; set; }
        public string? RequestedByName { get; set; }
        public string? NewPresenterId { get; set; }
        public string? NewPresenterName { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? Reason { get; set; }
        public DateTime? RequestAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? PaperId { get; set; }
        public string? SessionId { get; set; }
    }

    public class CreatePresenterChangeRequest
    {
        [Required]
        public string? TicketId { get; set; }
        [Required]
        public string? SessionId { get; set; }

        [Required]
        public string? PaperId { get; set; }

        [Required]
        public string? NewUserId { get; set; }

        [Required]
        public string? Reason { get; set; }
    }

    public class ApprovePresenterChangeRequest
    {
        [Required]
        public string? PresenterChangeRequestId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string? ReviewerComment { get; set; }
    }

    public class CreateSessionChangeRequest
    {

        [Required]
        public string? NewSessionId { get; set; }

        [Required]
        public string? TicketId { get; set; }
        [Required]
        public string? Reason { get; set; }
    }

    public class ApproveSessionChangeRequest
    {
        [Required]
        public string? SessionChangeRequestId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string? ReviewerComment { get; set; }
    }

    public class SessionChangeRequestResponse
    {
        public string? SessionChangeRequestId { get; set; }
        public string? CurrentSessionId { get; set; }
        public string? NewSessionId { get; set; }
        public string? PaperId { get; set; }
        public string? RequestedById { get; set; }
        public string? RequestedByName { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? GlobalStatusName { get; set; }
        public string? Reason { get; set; }
        public DateTime? RequestAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}