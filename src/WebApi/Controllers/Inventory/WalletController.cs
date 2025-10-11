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
}