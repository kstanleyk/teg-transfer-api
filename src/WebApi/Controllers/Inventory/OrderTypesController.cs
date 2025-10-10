using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Inventory.OrderType.Commands;
using Transfer.Application.Features.Inventory.OrderType.Dtos;
using Transfer.Application.Features.Inventory.OrderType.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class OrderTypesController(IMediator mediator) : ApiControllerBase<OrderTypesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.OrderType, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrderTypesQuery())));

    [HttpGet("{orderTypeId:guid}")]
    [MustHavePermission(AppFeature.OrderType, AppAction.Read)]
    public async Task<IActionResult> Get(Guid orderTypeId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrderTypeQuery { PublicId = orderTypeId })));

    [HttpPost]
    [MustHavePermission(AppFeature.OrderType, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateOrderTypeRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateOrderTypeCommand { OrderType = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.OrderType, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditOrderTypeRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditOrderTypeCommand { OrderType = request })));
}