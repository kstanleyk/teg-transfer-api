using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Inventory.Order.Commands;
using Transfer.Application.Features.Inventory.Order.Dtos;
using Transfer.Application.Features.Inventory.Order.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class OrdersController(IMediator mediator) : ApiControllerBase<OrdersController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Order, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrdersQuery())));

    [HttpGet("{orderId:guid}")]
    [MustHavePermission(AppFeature.Order, AppAction.Read)]
    public async Task<IActionResult> Get(Guid orderId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new OrderQuery { PublicId = orderId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Order, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateOrderCommand { Order = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Order, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditOrderRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditOrderCommand { Order = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Order, AppAction.Submit)]
    [Route("submit")]
    public async Task<IActionResult> Submit([FromBody] EditOrderRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new SubmitOrderCommand { Order = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Order, AppAction.Validate)]
    [Route("validate")]
    public async Task<IActionResult> Validate([FromBody] EditOrderRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new ValidateOrderCommand { Order = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Order, AppAction.Receive)]
    [Route("receive")]
    public async Task<IActionResult> Receive([FromBody] EditOrderRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new ReceiveOrderCommand { Order = request })));
}