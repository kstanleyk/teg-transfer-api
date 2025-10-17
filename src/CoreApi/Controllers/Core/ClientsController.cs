using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Client.Commands;
using TegWallet.Application.Features.Core.Client.Dto;
using TegWallet.CoreApi.Attributes;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
//[ApiVersion("2.0")]
public class ClientsController(IMediator mediator, IMapper mapper,IStringLocalizer<SharedResources> _localizer) : ApiControllerBase<ClientsController>
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

    //[MapToApiVersion("1.0")]
    //[HttpGet("culture")]
    //public IActionResult GetCulture()
    //{
    //    return Ok(new
    //    {
    //        Culture = System.Globalization.CultureInfo.CurrentCulture.Name,
    //        UICulture = System.Globalization.CultureInfo.CurrentUICulture.Name,
    //        ResourceName = _localizer["OrderCreatedSuccess"]
    //    });
    //}
}