using ConfRadar.Repositories;
using ConfRadar.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ConfRadar.Services.BackgroundJobs
{
    public class UpdateReviewerContractQuartzJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public UpdateReviewerContractQuartzJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var activeReviewContractList = await unitOfWork.ReviewerContractRepository.GetActiveReviewerContract();
            if (!activeReviewContractList.Any())
            {
                return;
            }
            var timeProviderService = scope.ServiceProvider.GetRequiredService<ITimeProviderService>();
            var dateNow = await timeProviderService.GetVietnamDate();

            var expiredReviewContractList = activeReviewContractList.Where(rc => rc.ExpireDay < dateNow).ToList();
            if (expiredReviewContractList.Any())
            {
                foreach (var contract in expiredReviewContractList)
                {
                    contract.IsActive = false;
                }
                await unitOfWork.ReviewerContractRepository.UpdateMutipleReviewerContractAsync(expiredReviewContractList);
            }
        }
    }
}
