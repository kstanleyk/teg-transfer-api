using System.Security.Claims;

namespace Transfer.WebApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}