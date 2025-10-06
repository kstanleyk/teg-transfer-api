using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.Supplier.Commands;
using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using Agrovet.Application.Features.Inventory.Supplier.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class SuppliersController(IMediator mediator) : ApiControllerBase<SuppliersController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.Supplier, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new SuppliersQuery())));

    [HttpGet("{itemCategoryId:guid}")]
    [MustHavePermission(AppFeature.Supplier, AppAction.Read)]
    public async Task<IActionResult> Get(Guid itemCategoryId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new SupplierQuery { PublicId = itemCategoryId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Supplier, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateSupplierCommand { Supplier = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.Supplier, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditSupplierRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditSupplierCommand { Supplier = request })));
}