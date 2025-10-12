using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Query;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.WebApi.Attributes;

namespace TegWallet.WebApi.Controllers.Inventory;

public class WalletController(IMediator mediator) : ApiControllerBase<WalletController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpPost("{clientId:guid}/deposit")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Deposit)]
    public async Task<IActionResult> DepositFunds(Guid clientId, [FromBody] DepositRequestDto request)
    {
        var command = new RequestDepositFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Reference,
            request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/deposit/approve")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> ApproveDeposit(Guid clientId, [FromBody] ApproveDepositDto request)
    {
        var command = new ApproveDepositFundsCommand(
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
        var command = new RejectDepositFundsCommand(
            clientId,
            request.LedgerId,
            request.Reason,
            request.RejectedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [HttpPost("{clientId:guid}/withdraw")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Withdraw)]
    public async Task<IActionResult> WithdrawFunds(Guid clientId, [FromBody] WithdrawalRequestDto request)
    {
        var command = new RequestWithdrawFundsCommand(clientId, request.Amount, request.CurrencyCode, request.Description);

        var result = await MediatorSender.Send(command);
        return Ok(result);
    }

    [HttpPost("{clientId:guid}/withdraw/approve")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> ApproveWithdrawal(Guid clientId, [FromBody] ApproveWithdrawalDto request)
    {
        var command = new ApproveWithdrawFundsCommand(
            clientId,
            request.LedgerId,
            request.ApprovedBy);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    [HttpPost("{clientId:guid}/withdraw/reject")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Approve)]
    public async Task<IActionResult> RejectWithdrawal(Guid clientId, [FromBody] RejectWithdrawalDto request)
    {
        var command = new RejectWithdrawFundsCommand(
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

    [HttpGet("{clientId:guid}/purchase-reservations")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<PagedResponse<PurchaseReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResponse<PurchaseReservationDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientPurchaseReservations(
        Guid clientId,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        PurchaseReservationStatus? reservationStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PurchaseReservationStatus>(status, true, out var parsedStatus))
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

    [HttpGet("{clientId:guid}/purchase-reservations/summary")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<PurchaseReservationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientPurchaseReservationSummary(Guid clientId)
    {
        var query = new GetClientPurchaseReservationSummaryQuery(clientId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("purchase-reservations/{reservationId:guid}")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<PurchaseReservationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PurchaseReservationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseReservationById(Guid reservationId)
    {
        var query = new GetPurchaseReservationByIdQuery(reservationId);

        return Ok(await Mediator.Send(query));
    }

    [HttpGet("purchase-reservations")]
    [MustHavePermission(AppFeature.Wallet, AppAction.Read)]
    [ProducesResponseType(typeof(Result<PagedResponse<PurchaseReservationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPurchaseReservations(
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
        PurchaseReservationStatus? reservationStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PurchaseReservationStatus>(status, true, out var parsedStatus))
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