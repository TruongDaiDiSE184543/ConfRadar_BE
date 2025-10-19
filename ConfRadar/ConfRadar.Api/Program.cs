using ConfRadar.Api;
using ConfRadar.Api.Middleware;
using ConfRadar.Repositories;
using ConfRadar.Services;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using System.Collections.Generic;
var builder = WebApplication.CreateBuilder(args);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddRepositories();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    await seedDataService.SeedRolesAsync();
    await seedDataService.SeedGlobalStatusesAsync();
    await seedDataService.SeedTransactionStatusAsync();
    await seedDataService.SeedPaymentMethodsAsync();
}
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
