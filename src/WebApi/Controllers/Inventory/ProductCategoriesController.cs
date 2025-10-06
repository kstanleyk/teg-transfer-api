using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.ProductCategory.Commands;
using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using Agrovet.Application.Features.Inventory.ProductCategory.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class ProductCategoriesController(IMediator mediator) : ApiControllerBase<ProductCategoriesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ProductCategoriesQuery())));

    [HttpGet("{itemCategoryId:guid}")]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Read)]
    public async Task<IActionResult> Get(Guid itemCategoryId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ProductCategoryQuery { PublicId = itemCategoryId })));

    [HttpPost]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateProductCategoryCommand { ProductCategory = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.ItemCategory, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditProductCategoryRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditProductCategoryCommand { ProductCategory = request })));
}