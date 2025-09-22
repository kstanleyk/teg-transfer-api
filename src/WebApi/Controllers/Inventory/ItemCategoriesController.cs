using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.ItemCategory.Commands;
using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using Agrovet.Application.Features.Inventory.ItemCategory.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class ItemCategoriesController(IMediator mediator) : ApiControllerBase<ItemCategoriesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ItemCategoriesQuery())));

    [HttpGet("{itemCategoryId:guid}")]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Read)]
    public async Task<IActionResult> Get(Guid itemCategoryId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ItemCategoryQuery { PublicId = itemCategoryId })));

    [HttpPost]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateItemCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateItemCategoryCommand { ItemCategory = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditItemCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditItemCategoryCommand { ItemCategory = request })));
}