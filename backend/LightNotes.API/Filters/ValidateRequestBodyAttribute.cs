using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LightNotes.API.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ValidateRequestBodyAttribute(string actionDescription) : ActionFilterAttribute
{
    private readonly string _actionDescription = actionDescription;
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<ValidateRequestBodyAttribute>>();
        var userId = context.HttpContext.User?.Identity?.Name ?? "Unknown";
        var methodName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown method";

        // Ім'я параметра, який приймає тіло запиту [FromBody]
        var bodyParamName = context.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body)?
            .Name;
        if (bodyParamName == null)
        {
            return;
        }

        var hasValidBody = context.ActionArguments.TryGetValue(bodyParamName, out var value) && value != null;

        // Повертаємо Bad Request, якщо тіло запиту не передано або воно порожнє
        if (!hasValidBody)
        {
            logger?.LogWarning("User {UserId} did not provide a request body for {MethodName}.", userId, methodName);
            context.Result = new BadRequestObjectResult(new
            {
                error = $"Empty request body for {_actionDescription}."
            });
        }
    }
}
