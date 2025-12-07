using ConfRadar.Services.BackgroundJobs;
using ConfRadar.Services.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Quartz;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configs)
        {

            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddSingleton<IRedisService, RedisService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<ISeedDataService, SeedDataService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IMomoService, MomoService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped<IConferenceService, ConferenceService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IDestinationService, DestinationService>();
            services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
            services.AddScoped<IConferencePriceTicketService, ConferencePriceTicketService>();
            services.AddScoped<IConferenceStepService, ConferenceStepService>();
            services.AddScoped<IConferenceCategoryService, ConferenceCategoryService>();
            services.AddScoped<IGlobalStatusService, GlobalStatusService>();
            services.AddScoped<IRankingCategoryService, RankingCategoryService>();
            services.AddScoped<IPaperService, PaperService>();
            services.AddScoped<IPaperAssignmentService, PaperAssignmentService>();
            services.AddScoped<IFullPaperService, FullPaperService>();
            services.AddScoped<IRevisionPaperService, RevisionPaperService>();
            services.AddScoped<ICameraReadyService, CameraReadyService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IConferenceStatusService, ConferenceStatusService>();
            services.AddScoped<IReviewStatusService, ReviewStatusService>();
            services.AddScoped<IConferenceTimelineService, ConferenceTimelineService>();
            services.AddScoped<IFavouriteConferenceService, FavouriteConferenceService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddHttpClient<NotificationService>();

            services.AddScoped<IZaloPayService, ZaloPayService>();
            services.AddScoped<IPayOsService, PayOsService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IAssigningPresenterSessionService, AssigningPresenterSessionService>();
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IQRCoderService, QRCoderService>();
            services.AddScoped<ITimeProviderService, TimeProviderService>();
            services.AddHttpClient<IOrcidService, OrcidService>();
            //services.AddScoped<IOrcidService, OrcidService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IReviewerService, ReviewerService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            var objectStorageSettings = configs.GetSection("ObjectStorageSettings").Get<ObjectStorageSettings>();
            services.AddSingleton<IMinioClient>(sp =>
            new Minio.MinioClient().WithEndpoint(objectStorageSettings!.EndPointAccess)
            .WithCredentials(objectStorageSettings!.AccessKey, objectStorageSettings!.SecretKey)
            .WithSSL(objectStorageSettings.Secure)
            .Build());
            services.AddSingleton<IObjectStorageFileService, ObjectStorageFileService>();

            var firebaseSettings = configs.GetSection("FirebaseSettings").Get<FirebaseSettings>();
            var credential = GoogleCredential.FromFile(firebaseSettings!.ServiceAccountPath);

            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = credential,
            });




            services.AddSingleton(FirebaseAuth.GetAuth(firebaseApp));
            //add jobs
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.AddQuartz(q =>
            {
                var notifyJobKey = new JobKey("NotifyWaitListQuartzJob");
                q.AddJob<NotifyWaitListQuartzJob>(opts => opts.WithIdentity(notifyJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(notifyJobKey)
                    .WithIdentity("NotifyWaitListTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever()));



                var resetWLJobKey = new JobKey("ResetNotifyWaitListQuartzJob");
                q.AddJob<ResetNotifyWaitListQuartzJob>(opts => opts.WithIdentity(resetWLJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(resetWLJobKey)
                    .WithIdentity("ResetNotifyWaitListTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(60).RepeatForever()));



                var updateRVCJobKey = new JobKey("UpdateReviewerContractQuartzJob");
                q.AddJob<UpdateReviewerContractQuartzJob>(opts => opts.WithIdentity(updateRVCJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(updateRVCJobKey)
                    .WithIdentity("UpdateReviewerContractQuartzTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()));



                var updateUCIJobKey = new JobKey("UpdateUserCheckInQuartzJob");
                q.AddJob<UpdateUserCheckInQuartzJob>(opts => opts.WithIdentity(updateUCIJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(updateUCIJobKey)
                    .WithIdentity("UpdateUserCheckInQuartzTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()));



                var updateConfCompletedJobKey = new JobKey("AutoUpdateConferenceStatusQuartzJob");
                q.AddJob<AutoUpdateConferenceStatusQuartzJob>(opts => opts.WithIdentity(updateConfCompletedJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(updateConfCompletedJobKey)
                    .WithIdentity("AutoUpdateConferenceStatusQuartzTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()));


                var updateExpiredPaperJobKey = new JobKey("UpdateExpiredPaperQuartzJob");
                q.AddJob<UpdateExpiredPaperQuartzJob>(opts => opts.WithIdentity(updateExpiredPaperJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(updateExpiredPaperJobKey)
                    .WithIdentity("UpdateExpiredPaperQuartzTrigger")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(60).RepeatForever()));


            });

            return services;
        }
    }
}
