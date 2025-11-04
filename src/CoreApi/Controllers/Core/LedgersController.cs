using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.Ledgers.Query;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class LedgersController(IMediator mediator) : ApiControllerBase<LedgersController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetLedgersV1()
    {
        var query = new GetLedgersQuery();
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingLedgersV1()
    {
        var query = new GetPendingLedgersQuery();
        return Ok(await Mediator.Send(query));
    }
}