namespace TegWallet.WalletApi.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class MustMatchClientAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[]? _routeParamNames;

    /// <summary>
    /// Specify one or more route parameter names to check against the authenticated user's ID.
    /// </summary>
    /// <param name="routeParamNames">Route parameter names, e.g. "clientId", "userId"</param>
    public MustMatchClientAttribute(params string[]? routeParamNames)
    {
        if (routeParamNames == null || routeParamNames.Length == 0)
        {
            _routeParamNames = ["clientId"]; // default
        }
        else
        {
            _routeParamNames = routeParamNames;
        }
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<MustMatchClientAttribute>)) as ILogger;

        try
        {
            // Extract the authenticated user's ID from claim ("sub" or NameIdentifier)
            var user = context.HttpContext.User;
            var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = CreateError(401, "unauthorized", "Invalid or missing authentication claim.");
                return;
            }

            // Check each specified route parameter
            foreach (var paramName in _routeParamNames!)
            {
                if (!context.ActionArguments.TryGetValue(paramName, out var paramValue))
                    continue; // skip missing route params

                if (paramValue is Guid routeId && routeId != userId)
                {
                    logger?.LogWarning(
                        "Unauthorized access attempt: user {UserId} tried to access {RouteParam}={RouteValue} at {Path}",
                        userId, paramName, routeId, context.HttpContext.Request.Path
                    );

                    context.Result = CreateError(403, "forbidden", $"You are not authorized to access this {paramName}.");
                    return;
                }
            }

            await next(); // ✅ proceed if all matched
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while enforcing MatchClientId attribute.");
            context.Result = CreateError(500, "server_error", "An unexpected error occurred while processing your request.");
        }
    }

    private JsonResult CreateError(int statusCode, string code, string message)
        => new(new { code, message }) { StatusCode = statusCode };
}
