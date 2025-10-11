using Microsoft.AspNetCore.Authorization;
using TegWallet.Application.Interfaces.Auth;

namespace TegWallet.WebApi.Permissions;

public class PermissionAuthorizationHandler(IUserPermissionRepository userPermissionRepository)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var subject = context.User.Claims
            .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?.Value;

        if (string.IsNullOrWhiteSpace(subject))
        {
            context.Fail();
            return;
        }

        var userPermissions = 
            await userPermissionRepository.GetPermissionsForUserAsync(subject);

        var requiredPermissions = userPermissions
            .Where(claim => claim == requirement.Permission);

        if (requiredPermissions.Any())
        {
            context.Succeed(requirement);
            await Task.CompletedTask;
        }
    }
}