using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.Wallets.Query;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class WalletsController(IMediator mediator) : ApiControllerBase<WalletsController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetWalletsV1()
    {
        var query = new GetWalletsQuery();
        return Ok(await Mediator.Send(query));
    }
}