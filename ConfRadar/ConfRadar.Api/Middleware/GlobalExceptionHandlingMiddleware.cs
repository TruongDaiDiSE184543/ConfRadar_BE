

using ConfRadar.Api.Responses;
using ConfRadar.Services.Exceptions;
using System.Text.Json;

namespace ConfRadar.Api.Middleware
{

    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                var statusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    ConfRadarAuthenticationException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };
                await HandleExceptionAsync(context, statusCode, ex.Message);
            }
        }
        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            var response = ApiResponse<object>.FailResponse(message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
