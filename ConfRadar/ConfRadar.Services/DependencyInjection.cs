using ConfRadar.Services.Services;
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
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<ITokenService, TokenService>();


            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IServiceManager, ServiceManager>();

            var objectStorageSettings = configs.GetSection("ObjectStorageSettings").Get<ObjectStorageSettings>();
            services.AddMinio(objectStorageSettings!.AccessKey, objectStorageSettings.SecretKey, ServiceLifetime.Singleton);
            services.AddSingleton<IObjectStorageFileService, ObjectStorageFileService>();

            return services;
        }
    }
}
