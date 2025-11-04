using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Clients.Command;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Features.Core.Clients.Query;
using TegWallet.CoreApi.Attributes;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class ClientsController(IMapper mapper) : ApiControllerBase<ClientsController>
{
    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetClientsV1()
    {
        var query = new GetClientsQuery();
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterClientV1([FromBody] RegisterClientDto dto)
    {
        var command = mapper.Map<RegisterClientCommand>(dto);
        var result = await MediatorSender.Send(command);
        return Ok(result);
    }
}