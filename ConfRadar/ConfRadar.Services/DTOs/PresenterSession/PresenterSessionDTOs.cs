using System.ComponentModel.DataAnnotations;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.DTOs.PresenterSession
{
    public class PresenterSessionResponse
    {
        public string? PresentAuthorId { get; set; }
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
        public string? NewPresenterId { get; set; }
        public string? GlobalStatusId { get; set; }
        public string? Reason { get; set; }
        public DateTime? RequestAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? PaperId { get; set; }
        public string? SessionId { get; set; }
    }

    public class CreatePresenterChangeRequest
    {
        [Required]
        public string? PaperId { get; set; }
        
        [Required]
        public string? SessionId { get; set; }
        
        [Required]
        public string? NewUserId { get; set; }
        
        [Required]
        public string? Reason { get; set; }
    }
}