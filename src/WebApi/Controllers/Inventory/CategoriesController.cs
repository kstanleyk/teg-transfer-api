using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Inventory.Category.Commands;
using Transfer.Application.Features.Inventory.Category.Dtos;
using Transfer.Application.Features.Inventory.Category.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class CategoriesController(IMediator mediator) : ApiControllerBase<CategoriesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Category, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new CategoriesQuery())));

    [HttpGet("{itemCategoryId:guid}")]
    [MustHavePermission(AppFeature.Category, AppAction.Read)]
    public async Task<IActionResult> Get(Guid itemCategoryId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new CategoryQuery { PublicId = itemCategoryId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Category, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateCategoryCommand { Category = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Category, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditCategoryCommand { Category = request })));
}