using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.ClientGroups.Command;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Features.Core.ClientGroups.Queries;
using TegWallet.Application.Helpers;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
[Route("api/client-groups")]
public class ClientGroupsController(IMediator mediator) : ApiControllerBase<ClientGroupsController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpGet("{clientGroupId:guid}")]
    [ProducesResponseType(typeof(Result<ClientGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ClientGroupDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientGroupByIdV1(Guid clientGroupId)
    {
        var query = new GetClientGroupByIdQuery(clientGroupId);
        var result = await Mediator.Send(query);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResponse<ClientGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllClientGroupsV1(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllClientGroupsQuery(isActive, searchTerm, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("status/{isActive:bool}")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ClientGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientGroupsByStatusV1(bool isActive)
    {
        var query = new GetClientGroupsByStatusQuery(isActive);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientGroupId:guid}/with-clients")]
    [ProducesResponseType(typeof(Result<ClientGroupWithClientsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ClientGroupWithClientsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientGroupWithClientsV1(Guid clientGroupId)
    {
        var query = new GetClientGroupWithClientsQuery(clientGroupId);
        var result = await Mediator.Send(query);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientGroupId:guid}/can-delete")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CanDeleteClientGroupV1(Guid clientGroupId)
    {
        var query = new CanDeleteClientGroupQuery(clientGroupId);
        var result = await Mediator.Send(query);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<ClientGroupCreatedResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ClientGroupCreatedResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClientGroupV1([FromBody] CreateClientGroupRequestDto request)
    {
        var command = new CreateClientGroupCommand(
            request.Name,
            request.Description,
            request.CreatedBy);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            var response = new ClientGroupCreatedResponseDto
            {
                ClientGroupId = result.Data,
                Message = result.Message
            };

            return Ok(Result<ClientGroupCreatedResponseDto>.Succeeded(response));
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{clientGroupId:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClientGroupV1(Guid clientGroupId, [FromBody] UpdateClientGroupRequestDto request)
    {
        var command = new UpdateClientGroupCommand(
            clientGroupId,
            request.Name,
            request.Description,
            request.UpdatedBy);

        var result = await Mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPatch("{clientGroupId:guid}/activate")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateClientGroupV1(Guid clientGroupId, [FromBody] ActivateClientGroupRequestDto request)
    {
        var command = new ActivateClientGroupCommand(
            clientGroupId,
            request.ActivatedBy);

        var result = await Mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPatch("{clientGroupId:guid}/deactivate")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateClientGroupV1(Guid clientGroupId, [FromBody] DeactivateClientGroupRequestDto request)
    {
        var command = new DeactivateClientGroupCommand(
            clientGroupId,
            request.DeactivatedBy,
            request.Reason);

        var result = await Mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}