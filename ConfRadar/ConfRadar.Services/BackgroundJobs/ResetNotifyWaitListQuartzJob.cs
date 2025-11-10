using ConfRadar.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ConfRadar.Services.BackgroundJobs
{
    public class ResetNotifyWaitListQuartzJob : IJob
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
