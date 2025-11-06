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
    public class ResetNotifyWaitListQuartzJob :IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public ResetNotifyWaitListQuartzJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try
            {
                await notificationService.ResetWaitList();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
