using System.Security.Claims;

namespace TegWallet.WalletApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}