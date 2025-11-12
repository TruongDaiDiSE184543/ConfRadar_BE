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
        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            var listNotifiedUser = await _unitOfWork.PaperWaitListRepository.NotifyWaitListAsync(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId, ExtensionHelper.GetVietnamTime());
            if (listNotifiedUser != null && listNotifiedUser.Count > 0)
            {
                var notifionListObj = new List<Notification>();
                foreach (var notfiedUser in listNotifiedUser)
                {
                    var userNotification = new ConfRadar.Repositories.Models.Notification()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = notfiedUser.UserId,
                        CreatedAt = ExtensionHelper.GetVietnamTime(),
                        ReadStatus = false,
                        Type = "Paper wait list",
                        Title = "Danh sách hàng đợi cho hội nghị",

                    };
                    string message = $"Hội nghị {notfiedUser.ConferenceName} hiện đã mở lại slot đăng ký.";
                    if (notfiedUser.ConferencePriceDetailList.Count > 0)
                    {
                        message += " Các vé hiện có: ";
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
            await _unitOfWork.PaperWaitListRepository.ResetUserWaitList(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId, ExtensionHelper.GetVietnamTime());
        }
    }
}
