using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record ApproveDepositCommand(
    Guid ClientId,
    Guid LedgerId,
    string? ApprovedBy = null) : IRequest<Result>;


public class ApproveDepositCommandHandler(IWalletRepository walletRepository, IClientRepository clientRepository)
    : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<ApproveDepositCommand, Result>
{
    public async Task<Result> Handle(ApproveDepositCommand command, CancellationToken cancellationToken)
    {
        var validator = new ApproveDepositCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        var validation = await ValidateClientAndWalletAsync(command.ClientId);
        if (!validation.Success)
            return Result.Failed(validation.Message);

        var result = await WalletRepository.ApproveDepositAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result.Failed("An unexpected error occurred while processing your transaction. Please try again.");

        return Result.Succeeded();
    }
}