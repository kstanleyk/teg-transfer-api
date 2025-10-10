using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Sales.PriceItem.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Sales;

public class PriceItemsController(IMediator mediator) : ApiControllerBase<PriceItemsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.PriceItem, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new PriceItemsQuery())));
}