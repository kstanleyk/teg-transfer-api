using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TegWallet.Application.Features.Core.Clients.Command;
using TegWallet.Application.Features.Core.Clients.Dto;

namespace TegWallet.WalletApi.Controllers.Core;

[ApiVersion("1.0")]
public class ClientsController(IMediator mediator, IMapper mapper,IStringLocalizer<SharedResources> localizer) : ApiControllerBase<ClientsController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterClientV1([FromBody] RegisterClientDto dto)
    {
        var command = mapper.Map<RegisterClientCommand>(dto);
        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("culture")]
    public IActionResult GetCulture()
    {
        return Ok(new
        {
            Culture = System.Globalization.CultureInfo.CurrentCulture.Name,
            UICulture = System.Globalization.CultureInfo.CurrentUICulture.Name,
            ResourceName = localizer["OrderCreatedSuccess"]
        });
    }
}