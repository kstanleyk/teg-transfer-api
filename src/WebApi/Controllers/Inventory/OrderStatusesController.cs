using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.OrderStatus.Commands;
using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using Agrovet.Application.Features.Inventory.OrderStatus.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class OrderStatusesController(IMediator mediator) : ApiControllerBase<OrderStatusesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.OrderStatus, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrderStatusesQuery())));

    [HttpGet("{orderStatusId:guid}")]
    [MustHavePermission(AppFeature.OrderStatus, AppAction.Read)]
    public async Task<IActionResult> Get(Guid orderStatusId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrderStatusQuery { PublicId = orderStatusId })));

    [HttpPost]
    [MustHavePermission(AppFeature.OrderStatus, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateOrderStatusRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateOrderStatusCommand { OrderStatus = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.OrderStatus, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditOrderStatusRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditOrderStatusCommand { OrderStatus = request })));
}