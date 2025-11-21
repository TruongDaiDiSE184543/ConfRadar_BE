using ConfRadar.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace ConfRadar.Api.Middleware
{
    public class BlockDisabledUserMiddleware
    {
        private readonly RequestDelegate _next;
        public BlockDisabledUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private static readonly string[] BlockedRoutes = new[]
        {
    "/api/payment/pay-tech",
    "/api/payment/pay-research-paper",
    "/api/payment/pay-research-as-attendee"
        };
        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (context.User?.Identity != null && context.User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(path) && BlockedRoutes.Any(r => path.StartsWith(r)))
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await unitOfWork.UserRepository.GetUserByUserId(userId);
                        if (user != null && user.IsActive == false)
                        {
                            await WriteApiError(context, StatusCodes.Status401Unauthorized, "Bạn không thể mua vì tài khoản đã bị vô hiệu hóa.");
                            return;
                        }
                    }
                }
            }
            await _next(context);
        }
        private static async Task WriteApiError(HttpContext context, int statusCode, string message)
        {
            var response = ConfRadar.Api.Responses.ApiResponse<object>.FailResponse(message);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

}
