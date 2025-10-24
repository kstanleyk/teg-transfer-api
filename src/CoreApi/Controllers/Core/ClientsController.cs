using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Client.Commands;
using TegWallet.Application.Features.Core.Client.Dto;
using TegWallet.Application.Features.Core.Client.Query;
using TegWallet.CoreApi.Attributes;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
//[ApiVersion("2.0")]
public class ClientsController(IMapper mapper) : ApiControllerBase<ClientsController>
{
    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetCurrenciesV1()
    {
        var query = new GetClientsQuery();
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    [MustHavePermission(AppFeature.Client, AppAction.Create)]
    public async Task<IActionResult> RegisterClientV1([FromBody] RegisterClientDto dto)
    {
        var command = mapper.Map<RegisterClientCommand>(dto);
        var result = await MediatorSender.Send(command);
        return Ok(result);
    }
}