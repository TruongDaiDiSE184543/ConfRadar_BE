using ConfRadar.Services.DTOs.Paper;

namespace ConfRadar.Services.Services
{
    public interface IPaperAssignmentService
    {
        Task<string> AssignAuthorToPaper(AssignAuthorToPaperRequest request);
        Task<string> AssignReviewerToPaper(AssignReviewerToPaperRequest request);
    }
}