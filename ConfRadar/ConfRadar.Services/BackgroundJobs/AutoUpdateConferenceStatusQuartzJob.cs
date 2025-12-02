using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.BackgroundJobs
{
    public class AutoUpdateConferenceStatusQuartzJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public AutoUpdateConferenceStatusQuartzJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var readyconferenceStatus = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var completedConferenceStatus = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Completed.GetDescription());
            if (readyconferenceStatus == null || completedConferenceStatus ==null)
            {
                return;
            }
            var readyConferences = await unitOfWork.ConferenceRepository.GetConferenceByStatus(readyconferenceStatus);
            if (readyConferences.Any())
            {
                var timeProviderService = scope.ServiceProvider.GetRequiredService<ITimeProviderService>();
                var dateNow = await timeProviderService.GetVietnamDate();
                

                var conferenceNeedToBeComplete = new List<Conference>();
                foreach(var conference in readyConferences)
                {
                    if (conference.EndDate.HasValue && conference.EndDate < dateNow)
                    {
                        conference.ConferenceStatusId = completedConferenceStatus.ConferenceStatusId;
                        conferenceNeedToBeComplete.Add(conference);
                    }
                }
                if (conferenceNeedToBeComplete.Any())
                {
                    await unitOfWork.ConferenceRepository.UpdateMutipleConferenceAsync(conferenceNeedToBeComplete);
                }

            }
        }
    }
}
