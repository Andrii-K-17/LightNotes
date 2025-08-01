using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace LightNotes.API.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LogOperationAttribute(string actionDescription) : ActionFilterAttribute
{
    private readonly string _actionDescription = actionDescription;
    private ILogger<LogOperationAttribute>? _logger;
    private string _methodName = "Unknown";
    private string _userId = "Unknown";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _logger = context.HttpContext.RequestServices.GetService<ILogger<LogOperationAttribute>>();
        _methodName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
        _userId = context.HttpContext.User?.Identity?.Name ?? "Unknown";

        _logger?.LogInformation("Started {Method} by user {User} ({Desc})", _methodName, _userId, _actionDescription);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (_logger == null)
        { 
            return;
        }

        if (context.Exception != null)
        {
            LogError(context.Exception);
            return;
        }

        var errorMessage = TryExtractErrorMessage(context.Result);
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            LogError(errorMessage: errorMessage);
        }
        else
        {
            _logger.LogInformation("Completed {Method} successfully by user {User} ({Desc})", _methodName, _userId, _actionDescription);
        }
    }

    private void LogError(Exception? ex = null, string? errorMessage = null)
    {
        string message = errorMessage ?? ex?.Message ?? "Unknown error";

        if (ex != null)
        {
            _logger?.LogError(ex, "{Method} failed for user {User}: {Message}", _methodName, _userId, message);
        }
        else
        {
            _logger?.LogError("{Method} failed for user {User}: {Message}", _methodName, _userId, message);
        }
    }

    private static string? TryExtractErrorMessage(IActionResult? result)
    {
        if (result is ObjectResult { Value: ProblemDetails problem })
        {
            return problem.Detail ?? problem.Title;
        }
        return null;
    }
}
