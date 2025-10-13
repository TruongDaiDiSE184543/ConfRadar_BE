using ConfRadar.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ConfRadar.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<ITokenService, TokenService>();


            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
