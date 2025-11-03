using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Shared.DTO.User;

namespace ConfRadar.Services.Services
{
    public interface INotificationService
    {
        Task NotifyWaitList();
    }
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task NotifyWaitList()
        {
            var readyConferenceStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var pendingWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Pending.GetDescription());
            var notifiedWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());
            if (readyConferenceStatus == null || pendingWaitListStatus==null || notifiedWaitListStatus==null)
            {
                return;
            }
            var listNotifiedUser = await _unitOfWork.PaperWaitListRepository.NotifyWaitListAsync(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId,ExtensionHelper.GetVietnamTime());
        
        
        }
    }
}
