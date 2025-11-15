using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;

namespace ConfRadar.Services.Services
{
    public interface INotificationService
    {
        Task NotifyWaitList();
        Task ResetWaitList();
    }
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        public NotificationService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
        }

        public async Task NotifyWaitList()
        {
            var readyConferenceStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var pendingWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Pending.GetDescription());
            var notifiedWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());
            if (readyConferenceStatus == null || pendingWaitListStatus == null || notifiedWaitListStatus == null)
            {
                return;
            }
            var listNotifiedUser = await _unitOfWork.PaperWaitListRepository.NotifyWaitListAsync(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId, await _timeProviderService.GetVietnamTime());
            if (listNotifiedUser != null && listNotifiedUser.Count > 0)
            {
                var notifionListObj = new List<Notification>();
                foreach (var notfiedUser in listNotifiedUser)
                {
                    var userNotification = new ConfRadar.Repositories.Models.Notification()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = notfiedUser.UserId,
                        CreatedAt = await _timeProviderService.GetVietnamTime(),
                        ReadStatus = false,
                        Type = "Paper wait list",
                        Title = "Danh sách hàng d?i cho h?i ngh?",

                    };
                    string message = $"H?i ngh? {notfiedUser.ConferenceName} hi?n dã m? l?i slot dang ký.";
                    if (notfiedUser.ConferencePriceDetailList.Count > 0)
                    {
                        message += " Các vé hi?n có: ";
                        foreach (var conferencePrice in notfiedUser.ConferencePriceDetailList)
                        {
                            message += $" Vé {conferencePrice.TicketName} — còn {conferencePrice.AvailableSlot}/{conferencePrice.TotalSlot} vé (giá {conferencePrice.TicketPrice}). ";
                        }
                    }
                    userNotification.Message = message;
                    notifionListObj.Add(userNotification);
                }
                if (notifionListObj.Count > 0)
                {
                    await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(notifionListObj);
                }

            }

        }

        public async Task ResetWaitList()
        {
            var readyConferenceStatus = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var pendingWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Pending.GetDescription());
            var notifiedWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Notified.GetDescription());

            if (readyConferenceStatus == null || pendingWaitListStatus == null || notifiedWaitListStatus == null)
            {
                return;
            }
            await _unitOfWork.PaperWaitListRepository.ResetUserWaitList(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId, await _timeProviderService.GetVietnamTime());
        }
    }
}
