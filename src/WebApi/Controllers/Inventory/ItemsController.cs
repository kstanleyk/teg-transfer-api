using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.Item.Commands;
using Agrovet.Application.Features.Inventory.Item.Dtos;
using Agrovet.Application.Features.Inventory.Item.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class ItemsController(IMediator mediator) : ApiControllerBase<ItemsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ItemsQuery())));

    [HttpGet("{departmentId}")]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get(string departmentId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ItemQuery { Id = departmentId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Item, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateItemCommand { Item = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Item, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditItemRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditItemCommand { Item = request })));
}