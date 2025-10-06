using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Sales.PriceItem.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Sales;

public class PriceItemsController(IMediator mediator) : ApiControllerBase<PriceItemsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.PriceItem, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new PriceItemsQuery())));
}