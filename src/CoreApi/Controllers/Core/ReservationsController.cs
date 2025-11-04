using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.Purchases.Query;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class ReservationsController(IMediator mediator) : ApiControllerBase<ReservationsController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetReservationsV1()
    {
        var query = new GetReservationsQuery();
        return Ok(await Mediator.Send(query));
    }
}