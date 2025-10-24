using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Query;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.WalletApi.Attributes;

namespace TegWallet.WalletApi.Controllers.Core;

[ApiVersion("1.0")]
public class WalletController(IMediator mediator) : ApiControllerBase<WalletController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/deposit")]
    [MustMatchClient]
    public async Task<IActionResult> DepositFundsV1(Guid clientId, [FromBody] DepositRequestDto request)
    {
        var command = new RequestDepositFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Reference,
            request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/withdraw")]
    [MustMatchClient]
    public async Task<IActionResult> WithdrawFundsV1(Guid clientId, [FromBody] WithdrawalRequestDto request)
    {
        var command = new RequestWithdrawFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/purchase/reserve")]
    [MustMatchClient]
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
    [HttpGet("{clientId:guid}/purchase/reservations/{reservationId:guid}")]
    [MustMatchClient]
    public async Task<IActionResult> GetPurchaseReservationV1(Guid clientId, Guid reservationId)
    {
        var query = new GetPurchaseReservationQuery(clientId, reservationId);
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}")]
    [MustMatchClient]
    public async Task<IActionResult> GetWalletV1(Guid clientId)
    {
        var query = new GetWalletByClientIdQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance")]
    [MustMatchClient]
    public async Task<IActionResult> GetWalletBalanceV1(Guid clientId)
    {
        var query = new GetWalletBalanceQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance/simple")]
    [MustMatchClient]
    public async Task<IActionResult> GetSimpleBalanceV1(Guid clientId)
    {
        var query = new GetSimpleBalanceQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/balance/check-sufficiency")]
    [MustMatchClient]
    public async Task<IActionResult> CheckBalanceSufficiencyV1(Guid clientId, [FromBody] CheckBalanceRequest request)
    {
        var query = new CheckBalanceSufficiencyQuery(clientId, request.Amount, request.CurrencyCode);
        return Ok(await Mediator.Send(query));
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{clientId:guid}/balance/history")]
    [MustMatchClient]
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
    [MustMatchClient]
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
    [MustMatchClient]
    public async Task<IActionResult> GetClientPurchaseReservationSummaryV1(Guid clientId)
    {
        var query = new GetClientPurchaseReservationSummaryQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("purchase-reservations/{reservationId:guid}")]
    public async Task<IActionResult> GetPurchaseReservationByIdV1(Guid reservationId)
    {
        var query = new GetPurchaseReservationByIdQuery(reservationId);

        return Ok(await Mediator.Send(query));
    }
}