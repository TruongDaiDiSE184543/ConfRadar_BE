using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ConfRadar.Services.BackgroundJobs
{
    public class UpdateUserCheckInQuartzJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public UpdateUserCheckInQuartzJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var pendingCheckInStatus = await unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            var expiredCheckInStatus = await unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Expired.GetDescription());


            var completedStatusConf = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Completed.GetDescription());
            var readyStatusConf = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var canceledStatusConf = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Cancelled.GetDescription());

            if (pendingCheckInStatus == null || expiredCheckInStatus == null || completedStatusConf == null || readyStatusConf == null || canceledStatusConf == null)
            {
                return;
            }
            var confStatuses = new List<ConferenceStatus>()
            {
                completedStatusConf,
                readyStatusConf,
                canceledStatusConf
            };
            var pendingUserCheckIn = await unitOfWork.UserCheckInRepository.GetUserCheckInByCheckInStatusAndConfStatuses(pendingCheckInStatus, confStatuses);
            if (pendingUserCheckIn.Any())
            {
                var timeProviderService = scope.ServiceProvider.GetRequiredService<ITimeProviderService>();
                var timeNow = await timeProviderService.GetVietnamTime();
                List<UserCheckIn> expireListUserCheckIn = new List<UserCheckIn>();
                foreach (var uci in pendingUserCheckIn)
                {
                    var userSession = uci.ConferenceSession;
                    if (userSession == null)
                    {
                        continue;
                    }
                    if (userSession.EndTime < timeNow)
                    {
                        uci.CheckinStatus = expiredCheckInStatus;
                        expireListUserCheckIn.Add(uci);
                    }


                }
                if (expireListUserCheckIn.Any())
                {
                    await unitOfWork.UserCheckInRepository.UpdateMutipleUserCheckInAsync(expireListUserCheckIn);
                }
            }
        }
    }
}
