using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record ApproveWithdrawFundsCommand(
    Guid ClientId,
    Guid LedgerId,
    string ApprovedBy = "System") : IRequest<Result>;

public class ApproveWithdrawFundsCommandHandler(IWalletRepository walletRepository, UserManager<Domain.Entity.Core.Client> userManager)
    : BaseWalletCommandHandler<TransactionDto>(walletRepository, userManager), IRequestHandler<ApproveWithdrawFundsCommand, Result>
{
    public async Task<Result> Handle(ApproveWithdrawFundsCommand fundsCommand, CancellationToken cancellationToken)
    {
        var validator = new ApproveWithdrawFundsCommandValidator();
        var validationResult = await validator.ValidateAsync(fundsCommand, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        var walletValidation = await ValidateClientAndWalletAsync(fundsCommand.ClientId);
        if (!walletValidation.Success)
            return Result.Failed(walletValidation.Message);

        var result = await WalletRepository.ApproveWithdrawFundsAsync(fundsCommand);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result.Failed("An unexpected error occurred while processing your transaction. Please try again.");

        return Result.Succeeded("Withdraw funds request approved successfully.");
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        UserManager.Dispose();
    }
}