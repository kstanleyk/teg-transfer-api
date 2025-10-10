using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transfer.Application.Authorization;
using Transfer.Application.Features.Client.Commands;
using Transfer.Application.Features.Client.Dto;
using Transfer.WebApi.Attributes;

namespace Transfer.WebApi.Controllers.Inventory;

public class ClientsController(IMediator mediator, IMapper mapper) : ApiControllerBase<ClientsController>
{
    public IMediator Mediator { get; } = mediator;

    //[HttpGet]
    //[MustHavePermission(AppFeature.Client, AppAction.Read)]
    //public async Task<IActionResult> Get() =>
    //    await GetActionResult(async () => Ok(await MediatorSender.Send(new WarehousesQuery())));

    //[HttpGet("{publicId:guid}")]
    //[MustHavePermission(AppFeature.Client, AppAction.Read)]
    //public async Task<IActionResult> Get(Guid publicId) =>
    //    await GetActionResult(async () => Ok(await MediatorSender.Send(new WarehouseQuery { PublicId = publicId })));

    [HttpPost]
    [MustHavePermission(AppFeature.Client, AppAction.Create)]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterClientDto dto)
    {
        var command = mapper.Map<RegisterClientCommand>(dto);
        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    //[HttpPut]
    //[MustHavePermission(AppFeature.Client, AppAction.Update)]
    //public async Task<IActionResult> Update([FromBody] EditWarehouseRequest request) => await GetActionResult(async () =>
    //    Ok(await MediatorSender.Send(new EditWarehouseCommand { Warehouse = request })));
}