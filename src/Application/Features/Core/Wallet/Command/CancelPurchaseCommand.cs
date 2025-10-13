using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

// Cancel purchase (admin cancels if needed)
public record CancelPurchaseCommand(
    Guid ReservationId,
    string Reason,
    string CancelledBy = "ADMIN") : IRequest<Result>;

public class CancelPurchaseCommandHandler(IWalletRepository walletRepository, IClientRepository clientRepository)
    : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<CancelPurchaseCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelPurchaseCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();
            throw new ValidationException(validationErrors);
        }

        var wallet = await WalletRepository.GetByReservationIdAsync(command.ReservationId);
        if (wallet == null)
            return Result.Failed("Wallet not found for reservation");

        var validation = await ValidateClientAndWalletAsync(wallet.ClientId, command.ReservationId);
        if (!validation.Success)
            return Result.Failed(validation.Message);

        var result = await WalletRepository.CancelPurchaseAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result.Failed("An unexpected error occurred while cancelling the purchase");

        return Result.Succeeded("Purchase service request cancelled successfully.");
    }
}