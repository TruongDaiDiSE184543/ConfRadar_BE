namespace ConfRadar.Api
{
    public static class DependencyInjection
    {
        public static void  AddApiConfig(this IServiceCollection services,IConfiguration configs)
        {
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
