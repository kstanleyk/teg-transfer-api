using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Auth.Permission.Queries;
using TegWallet.WalletApi.Services;

namespace TegWallet.WalletApi.Controllers.Auth;

public class PermissionsController(CurrentUserService currentUserService) : ApiControllerBase<PermissionsController>
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var permissions = await MediatorSender.Send(new PermissionsQuery
        {
            UserId = userId
        });

        return Ok(permissions);
    }
}