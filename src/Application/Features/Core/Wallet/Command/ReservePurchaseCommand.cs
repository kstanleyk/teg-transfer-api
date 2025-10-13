using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

// Reserve funds for purchase (client initiates)
public record ReservePurchaseCommand(
    Guid ClientId,
    decimal PurchaseAmount,
    decimal ServiceFeeAmount,
    string CurrencyCode,
    string Description,
    string SupplierDetails,
    string PaymentMethod) : IRequest<Result<ReservedPurchaseDto>>;

public class ReservePurchaseCommandHandler(
    IWalletRepository walletRepository,
    IClientRepository clientRepository)
    : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository),
        IRequestHandler<ReservePurchaseCommand, Result<ReservedPurchaseDto>>
{
    public async Task<Result<ReservedPurchaseDto>> Handle(ReservePurchaseCommand command, CancellationToken cancellationToken)
    {
        var validator = new ReservePurchaseCommandValidator();
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
            return Result<ReservedPurchaseDto>.Failed(validation.Message);

        var result = await WalletRepository.ReservePurchaseAsync(command);
        if(result.Status!= RepositoryActionStatus.Updated)
            return Result<ReservedPurchaseDto>.Failed("An unexpected error occurred while processing your purchase reservation");

        return Result<ReservedPurchaseDto>.Succeeded(result.Entity!, "Service purchase request completed successfully, pending admin validation");

    }
}