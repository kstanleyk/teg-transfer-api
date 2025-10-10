using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Inventory.Warehouse.Commands;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Application.Features.Inventory.Warehouse.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class WarehousesController(IMediator mediator) : ApiControllerBase<WarehousesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new WarehousesQuery())));

    [HttpGet("{publicId:guid}")]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get(Guid publicId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new WarehouseQuery { PublicId = publicId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Item, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateWarehouseCommand { Warehouse = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Item, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditWarehouseRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditWarehouseCommand { Warehouse = request })));
}