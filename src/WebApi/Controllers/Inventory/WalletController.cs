using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Query;
using TegWallet.Application.Helpers;
using TegWallet.WebApi.Attributes;

namespace TegWallet.WebApi.Controllers.Inventory;

public class WalletController(IMediator mediator) : ApiControllerBase<WalletController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpPost("{clientId:guid}/deposit")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Deposit)]
    public async Task<IActionResult> DepositFunds(Guid clientId, [FromBody] DepositRequestDto request)
    {
        var command = new DepositFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Reference,
            request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/withdraw")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Withdraw)]
    public async Task<IActionResult> WithdrawFunds(Guid clientId, [FromBody] WithdrawalRequestDto request)
    {
        var command = new WithdrawFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/deposit/approve")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> ApproveDeposit(Guid clientId, [FromBody] ApproveDepositDto request)
    {
        var command = new ApproveDepositCommand(
            clientId,
            request.LedgerId,
            request.ApprovedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [HttpPost("{clientId:guid}/deposit/reject")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> RejectDeposit(Guid clientId, [FromBody] RejectDepositDto request)
    {
        var command = new RejectDepositCommand(
            clientId,
            request.LedgerId,
            request.Reason,
            request.RejectedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [HttpPost("{clientId:guid}/purchase/reserve")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> ReservePurchase(Guid clientId, [FromBody] ReservePurchaseRequestDto request)
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

    [HttpPost("{clientId:guid}/purchase/approve")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> ApprovePurchase(Guid clientId, [FromBody] ApprovePurchaseRequestDto request)
    {
        var command = new ApprovePurchaseCommand(
            request.ReservationId,
            request.ProcessedBy);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/purchase/cancel")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> CancelPurchase(Guid clientId, [FromBody] CancelPurchaseRequestDto request)
    {
        var command = new CancelPurchaseCommand(
            request.ReservationId,
            request.Reason,
            request.CancelledBy);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpGet("{clientId:guid}/purchase/reservations/{reservationId:guid}")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> GetPurchaseReservation(Guid clientId, Guid reservationId)
    {
        var query = new GetPurchaseReservationQuery(clientId, reservationId);
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }

    [HttpGet("{clientId:guid}")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<WalletDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWallet(Guid clientId)
    {
        var query = new GetWalletByClientIdQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{clientId:guid}/balance")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<WalletBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<WalletBalanceDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletBalance(Guid clientId)
    {
        var query = new GetWalletBalanceQuery(clientId);
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{clientId:guid}/balance/simple")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<SimpleBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSimpleBalance(Guid clientId)
    {
        var query = new GetSimpleBalanceQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/balance/check-sufficiency")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(BalanceSufficiencyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBalanceSufficiency(Guid clientId, [FromBody] CheckBalanceRequest request)
    {
        var query = new CheckBalanceSufficiencyQuery(clientId, request.Amount, request.CurrencyCode);
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{clientId:guid}/balance/history")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(BalanceHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceHistory(
        Guid clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] BalanceHistoryPeriod period = BalanceHistoryPeriod.Daily)
    {
        var query = new GetBalanceHistoryQuery(clientId, fromDate.ToUtcKind(), toDate.ToUtcKind(), period);
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}