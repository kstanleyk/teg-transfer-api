using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

// Approve purchase (admin approves after processing)
public record ApprovePurchaseCommand(
    Guid ReservationId,
    string ProcessedBy = "ADMIN") : IRequest<Result>;

public class ApprovePurchaseCommandHandler(IWalletRepository walletRepository, IClientRepository clientRepository)
    : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<ApprovePurchaseCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseCommand command, CancellationToken cancellationToken)
    {
        var validator = new ApprovePurchaseCommandValidator();
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

        var result = await WalletRepository.ApprovePurchaseAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result.Failed("An unexpected error occurred while approving the purchase");

        return Result.Succeeded("Purchase service request approved successfully.");
    }
}