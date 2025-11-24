using ConfRadar.Api;
using ConfRadar.Api.Middleware;
using ConfRadar.Repositories;
using ConfRadar.Services;
using ConfRadar.Services.Services;
var builder = WebApplication.CreateBuilder(args);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddRepositories();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    await seedDataService.SeedRolesAsync();
    await seedDataService.SeedGlobalStatusesAsync();
    await seedDataService.SeedConferenceStatusesAsync();
    await seedDataService.SeedRankingCategoriesAsync();
    await seedDataService.SeedReviewStatusesAsync();
    await seedDataService.SeedPaperPhasesAsync();
    await seedDataService.SeedPaymentMethodsAsync();
    await seedDataService.SeedCheckInStatusAsync();
    await seedDataService.SeedWaitListStatusesAsync();
}
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<BlockDisabledUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
