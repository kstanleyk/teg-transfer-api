using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Inventory.Product.Commands;
using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Application.Features.Inventory.Product.Queries;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class ProductsController(IMediator mediator) : ApiControllerBase<ProductsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ProductsQuery())));

    [HttpGet("{publicId:guid}")]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> Get(Guid publicId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ProductQuery { PublicId = publicId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Item, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateProductCommand { Product = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Item, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditProductRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditProductCommand { Product = request })));

    [HttpGet]
    [Route("stockBalances")]
    [MustHavePermission(AppFeature.Item, AppAction.Read)]
    public async Task<IActionResult> StockBalance() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new ProductStockBalancesQuery())));
}