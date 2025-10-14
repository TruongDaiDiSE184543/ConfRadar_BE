using ConfRadar.Api.Common.Configurations;
using ConfRadar.Api.Filters;
using ConfRadar.Repositories.Data;
using ConfRadar.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Api
{
    public static class DependencyInjection
    {
        public static void AddApiConfig(this IServiceCollection services, IConfiguration configs)
        {
            services.Configure<AppSettingConfig.EmailSettings>(configs.GetSection("EmailSettings"));
            services.Configure<AppSettingConfig.JwtSettings>(configs.GetSection("JwtSettings"));


            services.AddDbContext<ConfRadarDbContext>(options =>
            options.UseNpgsql(configs.GetConnectionString("ConnectionStrings")));


            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddJwtAuthentication(configs);
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
        }
    }
}
