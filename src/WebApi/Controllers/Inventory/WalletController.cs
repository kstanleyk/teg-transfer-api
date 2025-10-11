using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Authorization;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
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
}