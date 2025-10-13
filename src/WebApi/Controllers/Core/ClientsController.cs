using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Client.Commands;
using TegWallet.Application.Features.Core.Client.Dto;
using TegWallet.WebApi.Attributes;

namespace TegWallet.WebApi.Controllers.Core;

[ApiVersion("1.0")]
//[ApiVersion("2.0")]
public class ClientsController(IMediator mediator, IMapper mapper) : ApiControllerBase<ClientsController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost]
    [MustHavePermission(AppFeature.Client, AppAction.Create)]
    public async Task<IActionResult> RegisterClientV1([FromBody] RegisterClientDto dto)
    {
        var command = mapper.Map<RegisterClientCommand>(dto);
        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    //[MapToApiVersion("2.0")]
    //[HttpPost]
    //[MustHavePermission(AppFeature.Client, AppAction.Create)]
    //public async Task<IActionResult> RegisterClientV2([FromBody] RegisterClientDto dto)
    //{
    //    var command = mapper.Map<RegisterClientCommand>(dto);
    //    var result = await MediatorSender.Send(command);
    //    return Ok(result);
    //}
}