using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Inventory.BottlingType.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class BottlingTypesController(IMediator mediator) : ApiControllerBase<BottlingTypesController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.BottlingType, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new BottlingTypesQuery())));
}