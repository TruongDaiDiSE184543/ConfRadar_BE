using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.Notification;
using FirebaseAdmin;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface INotificationService
    {
        Task NotifyWaitList();
        Task ResetWaitList();
        Task<bool> SendMobilePushAsync(string deviceToken, string title, string body);
        Task<bool> SendWebPushAsync(string fcmToken, string title, string body);
        Task<List<UserNotificationDetailResponse>> GetOwnNotification(string userId);
        Task<int> UpdateReadStatus(List<UpdateReadStatusRequest> request, string userId);
    }
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;

        private readonly IOptions<FirebaseSettings> _firebaseSettings;
        public NotificationService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService, IOptions<FirebaseSettings> firebaseSettings)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;

            _firebaseSettings = firebaseSettings;
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
            var timeNow = await _timeProviderService.GetVietnamTime();
            var listNotifiedUser = await _unitOfWork.PaperWaitListRepository.NotifyWaitListAsync(readyConferenceStatus.ConferenceStatusId, pendingWaitListStatus.WaitListStatusId, notifiedWaitListStatus.WaitListStatusId, timeNow);
            if (listNotifiedUser != null && listNotifiedUser.Any())
            {
                var notifionListObj = new List<Notification>();
                foreach (var notfiedUser in listNotifiedUser)
                {
                    var userNotification = new ConfRadar.Repositories.Models.Notification()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = notfiedUser.UserId,
                        CreatedAt = timeNow,
                        ReadStatus = false,
                        Type = "Paper wait list",
                        Title = "Danh sách hàng đợi cho hội nghị",

                    };
                    string message = $"Hội nghị {notfiedUser.ConferenceName} hiện đã mở lại slot đăng kí.";
                    if (notfiedUser.ConferencePriceDetailList.Any())
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
                if (notifionListObj.Any())
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









        private async Task<string> GetFirebaseAccessToken()
        {
            var credential = FirebaseApp.DefaultInstance.Options.Credential;
            if (credential == null)
            {
                throw new Exception("Credential firebase chưa được khởi tạo.");
            }
            ;
            var scope = credential.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            return await scope.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }
        public async Task<bool> SendMobilePushAsync(string deviceToken, string title, string body)
        {

            var accessToken = await GetFirebaseAccessToken();
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var message = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new { title, body },
                    android = new { priority = "high" }
                }
            };
            var json = JsonSerializer.Serialize(message);
            var url = $"https://fcm.googleapis.com/v1/projects/{_firebaseSettings.Value.ProjectId}/messages:send";
            var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendWebPushAsync(string fcmToken, string title, string body)
        {
            var message = new
            {
                message = new
                {
                    token = fcmToken,
                    notification = new
                    {
                        title = title,
                        body = body
                    },

                }
            };
            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(message);
            var accessToken = await GetFirebaseAccessToken();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"https://fcm.googleapis.com/v1/projects/{_firebaseSettings.Value.ProjectId}/messages:send";
            var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
            return response.IsSuccessStatusCode;
        }


        public async Task<List<UserNotificationDetailResponse>> GetOwnNotification(string userId)
        {
            var notification = await _unitOfWork.NotificationRepository.GetNotificationsByUserIdAsync(userId);
            var userNotification = notification.Select(n => new UserNotificationDetailResponse()
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt,
                ReadStatus = n.ReadStatus,
            }).ToList();
            return userNotification;
        }

        public async Task<int> UpdateReadStatus(List<UpdateReadStatusRequest> request, string userId)
        {
            if (!request.Any())
            {
                throw new BadRequestException("Danh sách update không được rỗng");
            }
            var ownNotifications = await _unitOfWork.NotificationRepository.GetNotificationsByUserIdAsync(userId);
            var ownNotificationDict = ownNotifications.ToDictionary(n => n.NotificationId, n => n);
            var notificationList = new List<Notification>();
            foreach (var updateReq in request)
            {

                if (!ownNotificationDict.ContainsKey(updateReq.NotificationId))
                {
                    throw new BadRequestException("Bạn chỉ có thể update thông báo của chính mình");
                }
                var currentNoti = ownNotificationDict[updateReq.NotificationId];
                currentNoti.ReadStatus = updateReq.ReadStatus;
                notificationList.Add(currentNoti);
            }
            return await _unitOfWork.NotificationRepository.UpdateMutipleNotificationAsync(notificationList);


        }
    }
}
