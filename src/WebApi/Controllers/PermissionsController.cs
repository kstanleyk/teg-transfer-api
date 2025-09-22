using Agrovet.Application.Features.Auth.Permission.Queries;
using Agrovet.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers;

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