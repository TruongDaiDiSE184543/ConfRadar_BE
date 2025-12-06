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
    public class UpdateExpiredPaperQuartzJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public UpdateExpiredPaperQuartzJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var timeProviderService = scope.ServiceProvider.GetRequiredService<ITimeProviderService>();

            var dateNow = await timeProviderService.GetVietnamDate();
            var timeNow = await timeProviderService.GetVietnamTime();


            var pendingGlobalStatus = await unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var rejectedGlobalStatus = await unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());

            var pendingReviewStatus = await unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());
            var rejectedReviewStatus = await unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());

            var readyConfStatus = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription());
            var cancelConfStatus = await unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Cancelled.GetDescription());

            if (pendingReviewStatus == null || readyConfStatus == null || cancelConfStatus == null 
                || pendingGlobalStatus == null || rejectedReviewStatus == null||rejectedGlobalStatus==null) 
            {
                return;
            }
            var confStatusesList = new List<ConferenceStatus>()
            {
                readyConfStatus,
                cancelConfStatus,
            };

            var expiredFullPaper = await unitOfWork.FullPaperRepository.GetExpiredFullPaper(dateNow, pendingReviewStatus, confStatusesList);
            var expiredCamReadies = await unitOfWork.CameraReadyRepository.GetExpiredCameraReadies(dateNow, pendingGlobalStatus, confStatusesList);

           
            if (expiredFullPaper.Any())
            {
                foreach (var fp in expiredFullPaper)
                {
                    fp.ReviewStatusId = rejectedReviewStatus.ReviewStatusId;
                    fp.ReviewAt = timeNow;
                    fp.Reason = "Full Paper đã quá hạn nộp, hệ thống auto reject";
                }
                await unitOfWork.FullPaperRepository.UpdateMutipleFullPaperAsync(expiredFullPaper);
            }
            if (expiredCamReadies.Any())
            {
                foreach (var cr in expiredCamReadies)
                {
                    cr.GlobalStatusId= rejectedGlobalStatus.GlobalStatusId;
                    cr.ReviewAt = timeNow;
                    cr.Reason = "Camera Ready đã quá hạn nộp, hệ thống auto reject";
                }
                await unitOfWork.CameraReadyRepository.UpdateMutipleCameraReadiesAsync(expiredCamReadies);
            }

        }
    }
}
