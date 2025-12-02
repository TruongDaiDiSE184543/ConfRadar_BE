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
            if (pendingCheckInStatus == null || expiredCheckInStatus == null)
            {
                return;
            }
            var pendingUserCheckIn = await unitOfWork.UserCheckInRepository.GetUserCheckInByCheckInStatus(pendingCheckInStatus);
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
