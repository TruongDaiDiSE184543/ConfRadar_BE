using ConfRadar.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConfRadar.Api.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Where(e => e.Value?.Errors.Count > 0).ToDictionary(e => e.Key, e => e.Value?.Errors.Select(err => err.ErrorMessage).ToArray());
                var response = ApiResponse<object>.ValidationErrorResponse(errors);
                context.Result = new BadRequestObjectResult(response);

            }
        }
    }
}
