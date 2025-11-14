using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Command;

public record RejectWithdrawFundsCommand(
    Guid ClientId,
    Guid LedgerId,
    string Reason,
    string RejectedBy = "System") : IRequest<Result>;

public class RejectWithdrawFundsCommandHandler(IWalletRepository walletRepository, IClientRepository clientRepository)
    : BaseWalletCommandHandler<LedgerDto>(walletRepository, clientRepository), IRequestHandler<RejectWithdrawFundsCommand, Result>
{
    public async Task<Result> Handle(RejectWithdrawFundsCommand fundsCommand, CancellationToken cancellationToken)
    {
        var validator = new RejectWithdrawFundsCommandValidator();
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

        var result = await WalletRepository.RejectWithdrawFundsAsync(fundsCommand);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result.Failed("An unexpected error occurred while processing your transaction. Please try again.");

        return Result.Succeeded("Withdraw funds request cancelled successfully.");
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        ClientRepository.Dispose();
    }
}