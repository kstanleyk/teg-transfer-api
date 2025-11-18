using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.ExchangeRates.Command;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Features.Core.ExchangeRates.Queries;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
public class ExchangeRatesController(IMediator mediator) : ApiControllerBase<ExchangeRatesController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpGet("active")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ActiveExchangeRateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ActiveExchangeRateDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveExchangeRatesV1(
        [FromQuery] string? baseCurrencyCode = null,
        [FromQuery] string? targetCurrencyCode = null,
        [FromQuery] RateType? rateType = null,
        [FromQuery] DateTime? asOfDate = null)
    {
        var query = new GetActiveExchangeRatesQuery(
            BaseCurrencyCode: baseCurrencyCode,
            TargetCurrencyCode: targetCurrencyCode,
            RateType: rateType,
            AsOfDate: asOfDate);

        var result = await Mediator.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("general")]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGeneralExchangeRateV1([FromBody] CreateGeneralExchangeRateRequestDto request)
    {
        var command = new CreateGeneralExchangeRateCommand(
            Currency.FromCode(request.BaseCurrencyCode),
            Currency.FromCode(request.TargetCurrencyCode),
            request.BaseCurrencyValue,
            request.TargetCurrencyValue,
            request.Margin,
            request.EffectiveFrom,
            request.CreatedBy,
            request.Source,
            request.EffectiveTo);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            var response = new ExchangeRateCreatedResponseDto
            {
                ExchangeRateId = result.Data,
                Message = result.Message
            };

            return Ok(Result<ExchangeRateCreatedResponseDto>.Succeeded(response));
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("group")]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroupExchangeRateV1([FromBody] CreateGroupExchangeRateRequestDto request)
    {
        var command = new CreateGroupExchangeRateCommand(
            Currency.FromCode(request.BaseCurrencyCode),
            Currency.FromCode(request.TargetCurrencyCode),
            request.BaseCurrencyValue,
            request.TargetCurrencyValue,
            request.Margin,
            request.ClientGroupId,
            request.EffectiveFrom,
            request.CreatedBy,
            request.Source,
            request.EffectiveTo);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            var response = new ExchangeRateCreatedResponseDto
            {
                ExchangeRateId = result.Data,
                Message = result.Message
            };

            return Ok(Result<ExchangeRateCreatedResponseDto>.Succeeded(response));
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("individual")]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExchangeRateCreatedResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIndividualExchangeRateV1([FromBody] CreateIndividualExchangeRateRequestDto request)
    {
        var command = new CreateIndividualExchangeRateCommand(
            Currency.FromCode(request.BaseCurrencyCode),
            Currency.FromCode(request.TargetCurrencyCode),
            request.BaseCurrencyValue,
            request.TargetCurrencyValue,
            request.Margin,
            request.ClientId,
            request.EffectiveFrom,
            request.CreatedBy,
            request.Source,
            request.EffectiveTo);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            var response = new ExchangeRateCreatedResponseDto
            {
                ExchangeRateId = result.Data,
                Message = result.Message
            };

            return Ok(Result<ExchangeRateCreatedResponseDto>.Succeeded(response));
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{exchangeRateId:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateExchangeRateV1(Guid exchangeRateId, [FromBody] UpdateExchangeRateRequestDto request)
    {
        var command = new UpdateExchangeRateCommand(
            exchangeRateId,
            request.NewBaseCurrencyValue,
            request.NewTargetCurrencyValue,
            request.NewMargin,
            request.UpdatedBy,
            request.Reason);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{exchangeRateId:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateExchangeRateV1(Guid exchangeRateId, [FromBody] DeactivateExchangeRateRequestDto request)
    {
        var command = new DeactivateExchangeRateCommand(
            exchangeRateId,
            request.DeactivatedBy,
            request.Reason);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPatch("{exchangeRateId:guid}/extend-validity")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExtendExchangeRateValidityV1(Guid exchangeRateId, [FromBody] ExtendExchangeRateValidityRequestDto request)
    {
        var command = new ExtendExchangeRateValidityCommand(
            exchangeRateId,
            request.NewEffectiveTo,
            request.UpdatedBy,
            request.Reason);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("applicable-with-tiers")]
    [ProducesResponseType(typeof(Result<ExchangeRateApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExchangeRateApplicationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetApplicableExchangeRateWithTiersV1(
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? clientGroupId,
        [FromQuery] string baseCurrencyCode,
        [FromQuery] string targetCurrencyCode,
        [FromQuery] decimal transactionAmount, // Target currency amount
        [FromQuery] DateTime? asOfDate = null)
    {
        var query = new GetApplicableExchangeRateWithTiersQuery(
            clientId,
            clientGroupId,
            baseCurrencyCode,
            targetCurrencyCode,
            transactionAmount,
            asOfDate);

        var result = await Mediator.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }


    [MapToApiVersion("1.0")]
    [HttpPost("{exchangeRateId}/tiers")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ManageExchangeRateTiersV1(
        [FromRoute] Guid exchangeRateId,
        [FromBody] ManageExchangeRateTiersRequestDto request)
    {
        var command = new ManageExchangeRateTiersCommand(
            exchangeRateId,
            request.Tiers);

        var result = await Mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    public record ManageExchangeRateTiersRequestDto(
        List<ExchangeRateTierRequestDto> Tiers);
}