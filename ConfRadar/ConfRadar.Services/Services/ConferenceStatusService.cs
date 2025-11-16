using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;

namespace ConfRadar.Services.Services
{
    public interface IConferenceStatusService
    {
        Task<bool> IsStatusTransitionValidAsync(string currentStatus, string newStatus);
        Task<ConferenceStatus?> GetConferenceStatusByNameAsync(string statusName);
        Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync(string? userId);
        Task<List<ConferenceStatus>> GetAllConferenceStatusesForCustomerAsync();
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
                { "Draft", new List<string>{ "Pending" , "Deleted"} },
                { "Pending", new List<string> { "Preparing", "Rejected" , "Deleted" } },
                { "Preparing", new List<string> { "Ready",  "Deleted" } },
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

        public async Task<List<ConferenceStatus>> GetAllConferenceStatusesAsync(string? userId)
        {
            var conferenceStatus = await _unitOfWork.ConferenceStatusRepository.GetAllConferenceStatusesAsync();
            var draftStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Draft.GetDescription());
            var pendingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Pending.GetDescription());
            var preparingStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Preparing.GetDescription());
            var deletedStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Deleted.GetDescription());
            var rejectedStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Rejected.GetDescription());

            if (!string.IsNullOrEmpty(userId))
            {
                var userRole = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);
                var roleOfUser = userRole.Select(x => x.Role.RoleName).ToList();
                if (!roleOfUser.Any()) throw new Exception($"User với ID {userId} không thuộc về role nào");
                if (roleOfUser.Contains(SystemRoleEnum.ConferenceOrganizer.ToString()))
                {
                    conferenceStatus.Remove(draftStatus);
                }
                else if (roleOfUser.Contains(SystemRoleEnum.Customer.ToString())) 
                {
                    conferenceStatus.Remove(draftStatus);
                    conferenceStatus.Remove(deletedStatus);
                    conferenceStatus.Remove(pendingStatus);
                    conferenceStatus.Remove(preparingStatus);
                    conferenceStatus.Remove(rejectedStatus);
                }
            }
            else
            {
                conferenceStatus.Remove(draftStatus);
                conferenceStatus.Remove(deletedStatus);
                conferenceStatus.Remove(pendingStatus);
                conferenceStatus.Remove(preparingStatus);
                conferenceStatus.Remove(rejectedStatus);

            }
                return conferenceStatus;
        }

        public Task<List<ConferenceStatus>> GetAllConferenceStatusesForCustomerAsync()
        {
            throw new NotImplementedException();
        }
    }
}