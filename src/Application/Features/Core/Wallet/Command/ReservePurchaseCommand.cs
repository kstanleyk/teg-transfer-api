using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;

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
    : IRequestHandler<ReservePurchaseCommand, Result<ReservedPurchaseDto>>
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

        var client = await clientRepository.GetAsync(command.ClientId);
        if (client == null)
            return Result<ReservedPurchaseDto>.Failed("Client not found");

        if (client.Status != ClientStatus.Active)
            return Result<ReservedPurchaseDto>.Failed("Client account is not active");

        var wallet = await walletRepository.GetByClientIdAsync(command.ClientId);
        if (wallet == null)
            return Result<ReservedPurchaseDto>.Failed("Wallet not found for client");

        try
        {
            var results = await walletRepository.ReservePurchaseAsync(command);

            return Result<ReservedPurchaseDto>.Succeeded(results.Entity);
        }
        catch (DomainException ex)
        {
            return Result<ReservedPurchaseDto>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            // Log exception
            return Result<ReservedPurchaseDto>.Failed("An unexpected error occurred while processing your purchase reservation");
        }
    }
}