using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.PackagingType.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class PackagingTypesController(IMediator mediator) : ApiControllerBase<PackagingTypesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.PackagingType, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new PackagingTypesQuery())));
}