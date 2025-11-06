using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Features.Core.ExchangeRates.Queries;
using TegWallet.Application.Features.Core.Ledgers.Query;
using TegWallet.Application.Features.Core.Wallets.Command;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Query;
using TegWallet.Application.Helpers;
using TegWallet.CoreApi.Attributes;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.CoreApi.Controllers.Core;

[ApiVersion("1.0")]
//[ApiVersion("2.0")]
public class WalletController(IMediator mediator) : ApiControllerBase<WalletController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/deposit")]
    public async Task<IActionResult> DepositFundsV1(Guid clientId, [FromBody] DepositRequestDto request)
    {
        var command = new RequestDepositFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Reference,
            request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/deposit/approve")]
    public async Task<IActionResult> ApproveDepositV1(Guid clientId, [FromBody] ApproveDepositDto request)
    {
        var command = new ApproveDepositFundsCommand(
            clientId,
            request.LedgerId,
            request.ApprovedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/deposit/reject")]
    public async Task<IActionResult> RejectDepositV1(Guid clientId, [FromBody] RejectDepositDto request)
    {
        var command = new RejectDepositFundsCommand(
            clientId,
            request.LedgerId,
            request.Reason,
            request.RejectedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/withdraw")]
    public async Task<IActionResult> WithdrawFundsV1(Guid clientId, [FromBody] WithdrawalRequestDto request)
    {
        var command = new RequestWithdrawFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/withdraw/approve")]
    public async Task<IActionResult> ApproveWithdrawalV1(Guid clientId, [FromBody] ApproveWithdrawalDto request)
    {
        var command = new ApproveWithdrawFundsCommand(
            clientId,
            request.LedgerId,
            request.ApprovedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/withdraw/reject")]
    public async Task<IActionResult> RejectWithdrawalV1(Guid clientId, [FromBody] RejectWithdrawalDto request)
    {
        var command = new RejectWithdrawFundsCommand(
            clientId,
            request.LedgerId,
            request.Reason,
            request.RejectedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/purchase/reserve")]
    public async Task<IActionResult> ReservePurchaseV1(Guid clientId, [FromBody] ReservePurchaseRequestDto request)
    {
        var command = new ReservePurchaseCommand(
            clientId,
            request.PurchaseAmount,
            request.ServiceFeeAmount,
            request.CurrencyCode,
            request.Description,
            request.SupplierDetails,
            request.PaymentMethod);

        var result = await MediatorSender.Send(command);

        if (result.Success)
        {
            var response = new ReservedPurchaseResponseDto
            {
                ReservationId = result.Data.ReservationId,
                PurchaseLedgerId = result.Data.PurchaseLedgerId,
                ServiceFeeLedgerId = result.Data.ServiceFeeLedgerId,
                PurchaseAmount = result.Data.PurchaseAmount,
                ServiceFeeAmount = result.Data.ServiceFeeAmount,
                TotalAmount = result.Data.TotalAmount,
                CurrencyCode = result.Data.CurrencyCode,
                Description = result.Data.Description,
                SupplierDetails = result.Data.SupplierDetails,
                PaymentMethod = result.Data.PaymentMethod,
                Status = result.Data.Status,
                CreatedAt = result.Data.CreatedAt
            };

            return Ok(Result<ReservedPurchaseResponseDto>.Succeeded(response));
        }

        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/purchase/approve")]
    public async Task<IActionResult> ApprovePurchaseV1(Guid clientId, [FromBody] ApprovePurchaseRequestDto request)
    {
        var command = new ApprovePurchaseCommand(
            request.ReservationId,
            request.ProcessedBy);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/purchase/cancel")]
    public async Task<IActionResult> CancelPurchaseV1(Guid clientId, [FromBody] CancelPurchaseRequestDto request)
    {
        var command = new CancelPurchaseCommand(
            request.ReservationId,
            request.Reason,
            request.CancelledBy);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/purchase/reservations/{reservationId:guid}")]
    public async Task<IActionResult> GetPurchaseReservationV1(Guid clientId, Guid reservationId)
    {
        var query = new GetPurchaseReservationQuery(clientId, reservationId);
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/transactions/recent")]
    public async Task<IActionResult> GetRecentTransactionsV1(Guid clientId, [FromQuery] int limit = 5)
    {
        var query = new GetRecentLedgersQuery(clientId, limit);
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/transactions/pending")]
    public async Task<IActionResult> GetPendingTransactionsV1(Guid clientId)
    {
        var query = new GetPendingClientLedgersQuery(clientId);
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}")]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status400BadRequest)]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    public async Task<IActionResult> GetWalletV1(Guid clientId)
    {
        var query = new GetWalletByClientIdQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance")]
    [ProducesResponseType(typeof(Result<WalletBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<WalletBalanceDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletBalanceV1(Guid clientId)
    {
        var query = new GetWalletBalanceQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/exchange-rates/{baseCurrencyCode}/{targetCurrencyCode}")]
    [ProducesResponseType(typeof(Result<ExchangeRateDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExchangeRateDto?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ExchangeRateDto?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClientExchangeRateV1(
            Guid clientId,
            string baseCurrencyCode,
            string targetCurrencyCode,
            DateTime? asOfDate = null)
    {
        try
        {
            // Convert string currency codes to Currency objects
            var baseCurrency = Currency.FromCode(baseCurrencyCode);
            var targetCurrency = Currency.FromCode(targetCurrencyCode);

            var query = new GetClientExchangeRateQuery(clientId, baseCurrency, targetCurrency, asOfDate);
            var result = await Mediator.Send(query);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // Handle invalid currency codes
            return BadRequest(Result<ExchangeRateDto?>.Failed($"Invalid currency code: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                Result<ExchangeRateDto?>.Failed($"Internal server error: {ex.Message}"));
        }
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/exchange-rates/{baseCurrencyCode}")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ExchangeRateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ExchangeRateDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ExchangeRateDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClientAvailableRatesV1(
        Guid clientId,
        string baseCurrencyCode,
        DateTime? asOfDate = null)
    {
        try
        {
            // Convert string currency code to Currency object
            var baseCurrency = Currency.FromCode(baseCurrencyCode);

            var query = new GetClientAvailableRatesQuery(clientId, baseCurrency, asOfDate);
            var result = await Mediator.Send(query);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // Handle invalid currency code
            return BadRequest(Result<IReadOnlyList<ExchangeRateDto>>.Failed($"Invalid currency code: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                Result<IReadOnlyList<ExchangeRateDto>>.Failed($"Internal server error: {ex.Message}"));
        }
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance/simple")]
    [ProducesResponseType(typeof(Result<SimpleBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSimpleBalanceV1(Guid clientId)
    {
        var query = new GetSimpleBalanceQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/balance/check-sufficiency")]
    [ProducesResponseType(typeof(BalanceSufficiencyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBalanceSufficiencyV1(Guid clientId, [FromBody] CheckBalanceRequest request)
    {
        var query = new CheckBalanceSufficiencyQuery(clientId, request.Amount, request.CurrencyCode);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance/history")]
    [ProducesResponseType(typeof(BalanceHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceHistoryV1(
        Guid clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] BalanceHistoryPeriod period = BalanceHistoryPeriod.Daily)
    {
        var query = new GetBalanceHistoryQuery(clientId, fromDate.ToUtcKind(), toDate.ToUtcKind(), period);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/purchase-reservations")]
    [ProducesResponseType(typeof(Result<PagedResponse<ReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResponse<ReservationDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientPurchaseReservationsV1(
        Guid clientId,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        ReservationStatus? reservationStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var parsedStatus))
        {
            reservationStatus = parsedStatus;
        }

        var query = new GetClientPurchaseReservationsQuery(
            clientId,
            reservationStatus,
            page,
            pageSize,
            sortBy,
            sortDescending);

        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/purchase-reservations/summary")]
    [ProducesResponseType(typeof(Result<PurchaseReservationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientPurchaseReservationSummaryV1(Guid clientId)
    {
        var query = new GetClientPurchaseReservationSummaryQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("purchase-reservations/{reservationId:guid}")]
    [ProducesResponseType(typeof(Result<ReservationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ReservationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseReservationByIdV1(Guid reservationId)
    {
        var query = new GetPurchaseReservationByIdQuery(reservationId);

        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("purchase-reservations")]
    [ProducesResponseType(typeof(Result<PagedResponse<ReservationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPurchaseReservationsV1(
        [FromQuery] Guid? clientId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] string? supplierSearch = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        ReservationStatus? reservationStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var parsedStatus))
        {
            reservationStatus = parsedStatus;
        }

        var query = new SearchPurchaseReservationsQuery(
            clientId,
            reservationStatus,
            paymentMethod,
            supplierSearch,
            fromDate?.ToUtcKind(),
            toDate?.ToUtcEndOfDay(),
            minAmount,
            maxAmount,
            page,
            pageSize,
            sortBy,
            sortDescending);

        var result = await Mediator.Send(query);
        return Ok(result);
    }
}