namespace TegWallet.WalletApi.Attributes;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entity.Auth;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class MustMatchClientAttribute : TypeFilterAttribute
{
    public MustMatchClientAttribute(params string[]? routeParamNames)
        : base(typeof(MustMatchClientFilter))
    {
        if (routeParamNames == null || routeParamNames.Length == 0)
        {
            Arguments = [new[] { "clientId" }]; // default
        }
        else
        {
            Arguments = [routeParamNames];
        }
    }
}


public class MustMatchClientFilter(
    UserManager<ApplicationUser> userManager,
    ILogger<MustMatchClientFilter> logger,
    string[] routeParamNames)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            // Get user ID from claims
            var user = context.HttpContext.User;
            var userIdClaim = user.FindFirst("sub")?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = CreateError(401, "unauthorized", "Invalid or missing authentication claim.");
                return;
            }

            // Optional: load user from database, since you now have _userManager available
            var dbUser = await userManager.FindByIdAsync(userId.ToString());
            if (dbUser == null)
            {
                context.Result = CreateError(401, "unauthorized", "User not found.");
                return;
            }

            // Validate route values
            foreach (var paramName in routeParamNames)
            {
                if (!context.ActionArguments.TryGetValue(paramName, out var paramValue))
                    continue;

                if (paramValue is Guid routeId && routeId != dbUser.ClientId)
                {
                    logger.LogWarning(
                        "Unauthorized access: User {UserId} tried to access {Param}={Value} at {Path}",
                        userId, paramName, routeId, context.HttpContext.Request.Path
                    );

                    context.Result = CreateError(403, "forbidden", $"You are not authorized to access this {paramName}.");
                    return;
                }
            }

            await next(); // everything ok, continue
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MustMatchClientFilter processing error");
            context.Result = CreateError(500, "server_error", "Unexpected server error.");
        }
    }


    private static JsonResult CreateError(int statusCode, string code, string message)
        => new(new { code, message }) { StatusCode = statusCode };
}

