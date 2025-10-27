using ConfRadar.Services.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
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

            //services.AddScoped<IConferenceService, ConferenceService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IDestinationService, DestinationService>();
            services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
            services.AddScoped<IConferencePriceTicketService, ConferencePriceTicketService>();
            services.AddScoped<IConferenceStepService, ConferenceStepService>();
            services.AddScoped<IConferenceCategoryService, ConferenceCategoryService>();
            services.AddScoped<IGlobalStatusService, GlobalStatusService>();
            services.AddScoped<IPaperService, PaperService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IServiceManager, ServiceManager>();


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
                Credential = credential
            });
            services.AddSingleton(FirebaseAuth.GetAuth(firebaseApp));




            return services;
        }
    }
}
