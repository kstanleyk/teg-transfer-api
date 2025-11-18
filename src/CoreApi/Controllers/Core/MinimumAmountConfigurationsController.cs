using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Queries;
using TegWallet.Application.Helpers;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class MinimumAmountConfigurationsController :  ApiControllerBase<MinimumAmountConfigurationsController>
{
    [MapToApiVersion("1.0")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationCreatedResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationCreatedResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMinimumAmountConfigurationV1(
        [FromBody] CreateMinimumAmountConfigurationRequestDto request)
    {
        var command = new CreateMinimumAmountConfigurationCommand(
            request.BaseCurrencyCode,
            request.TargetCurrencyCode,
            request.MinimumAmount,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.CreatedBy);

        var result = await MediatorSender.Send(command);

        if (result.Success)
        {
            var response = new MinimumAmountConfigurationCreatedResponseDto
            {
                ConfigurationId = result.Data
            };

            return Ok(Result<MinimumAmountConfigurationCreatedResponseDto>.Succeeded(response));
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{configurationId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMinimumAmountConfigurationV1(
        [FromRoute] Guid configurationId,
        [FromBody] UpdateMinimumAmountConfigurationRequestDto request)
    {
        var command = new UpdateMinimumAmountConfigurationCommand(
            configurationId,
            request.MinimumAmount,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.UpdatedBy);

        var result = await MediatorSender.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{configurationId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateMinimumAmountConfigurationV1(
        [FromRoute] Guid configurationId,
        [FromBody] DeactivateMinimumAmountConfigurationRequestDto request)
    {
        var command = new DeactivateMinimumAmountConfigurationCommand(
            configurationId,
            request.Reason,
            request.DeactivatedBy);

        var result = await MediatorSender.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<MinimumAmountConfigurationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMinimumAmountConfigurationsV1(
        [FromQuery] string? baseCurrencyCode = null,
        [FromQuery] string? targetCurrencyCode = null,
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] bool activeOnly = true)
    {
        var query = new GetMinimumAmountConfigurationsQuery(
            baseCurrencyCode,
            targetCurrencyCode,
            asOfDate,
            activeOnly);

        var result = await MediatorSender.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{configurationId}")]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMinimumAmountConfigurationByIdV1([FromRoute] Guid configurationId)
    {
        var query = new GetMinimumAmountConfigurationByIdQuery(configurationId);
        var result = await MediatorSender.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("applicable")]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MinimumAmountConfigurationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetApplicableMinimumAmountConfigurationV1(
        [FromQuery] string baseCurrencyCode,
        [FromQuery] string targetCurrencyCode,
        [FromQuery] DateTime? asOfDate = null)
    {
        var query = new GetApplicableMinimumAmountConfigurationQuery(
            baseCurrencyCode,
            targetCurrencyCode,
            asOfDate);

        var result = await MediatorSender.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}