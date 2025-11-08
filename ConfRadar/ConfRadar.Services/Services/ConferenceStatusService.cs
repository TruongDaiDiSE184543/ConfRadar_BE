using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStatusService
    {
        Task<bool> IsStatusTransitionValidAsync(string currentStatus, string newStatus);
        Task<ConferenceStatus?> GetConferenceStatusByNameAsync(string statusName);
        Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync();
    }

    public class ConferenceStatusService : IConferenceStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConferenceStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsStatusTransitionValidAsync(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Preparing", "Rejected" } },
                { "Preparing", new List<string> { "Ready", "Cancelled" } },
                { "Ready", new List<string> { "OnHold", "Completed" } },
                { "OnHold", new List<string> { "Ready", "Cancelled" } },
                { "Completed", new List<string>() } // No transitions from Completed
            };

            // Check if current status exists in our transitions map
            if (!validTransitions.ContainsKey(currentStatus))
            {
                return false;
            }

            // Check if the new status is in the list of allowed transitions from current status
            var allowedTransitions = validTransitions[currentStatus];
            return allowedTransitions.Contains(newStatus);
        }

        public async Task<ConferenceStatus?> GetConferenceStatusByNameAsync(string statusName)
        {
            var statuses = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
            return statuses.FirstOrDefault(s => s.ConferenceStatusName?.Equals(statusName, StringComparison.OrdinalIgnoreCase) == true);
        }

        public async Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync()
        {
            return await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusAsync();
        }
    }
}