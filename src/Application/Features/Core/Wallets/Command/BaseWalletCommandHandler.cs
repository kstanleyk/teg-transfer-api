using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Wallets.Command;

public abstract class BaseWalletCommandHandler<TResult>(
    IWalletRepository walletRepository,
    IClientRepository clientRepository): RequestHandlerBase
{
    protected readonly IWalletRepository WalletRepository = walletRepository;
    protected readonly IClientRepository ClientRepository = clientRepository;

    protected async Task<Result<(Domain.Entity.Core.Client client, Domain.Entity.Core.Wallet wallet)>> ValidateClientAndWalletAsync(Guid clientId)
    {
        var client = await ClientRepository.GetAsync(clientId);
        if (client == null)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Client not found");

        if (client.Status != ClientStatus.Active)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Client account is not active");

        var wallet = await WalletRepository.GetByClientIdAsync(clientId);
        if (wallet == null)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Wallet not found for client");

        return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Succeeded((client, wallet));
    }

    protected Result<Domain.ValueObjects.Currency> ValidateCurrencyAsync(Domain.Entity.Core.Wallet wallet, string currencyCode)
    {

        var currency = Domain.ValueObjects.Currency.FromCode(currencyCode);
        if (currency != wallet.BaseCurrency)
            return Result<Domain.ValueObjects.Currency>.Failed($"Transaction currency ({currency.Code}) must match wallet's base currency ({wallet.BaseCurrency.Code})");

        return Result<Domain.ValueObjects.Currency>.Succeeded(currency);
    }

    protected async Task<Result<(Domain.Entity.Core.Client client, Domain.Entity.Core.Wallet wallet)>> ValidateClientAndWalletAsync(Guid clientId, Guid reservationId)
    {
        var client = await ClientRepository.GetAsync(clientId);
        if (client == null)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Client not found");

        if (client.Status != ClientStatus.Active)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Client account is not active");

        var wallet = await WalletRepository.GetByReservationIdAsync(reservationId);
        if (wallet == null)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Wallet not found for reservation");

        if (wallet.ClientId != clientId)
            return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Failed("Reservation does not belong to the specified client");

        return Result<(Domain.Entity.Core.Client, Domain.Entity.Core.Wallet)>.Succeeded((client, wallet));
    }

    protected async Task<LedgerDto> UpdateClient(LedgerDto ledgerDto,  Guid walletId)
    {
        var wallet = await WalletRepository.GetAsync(walletId);
        if (wallet == null)
        {
            return ledgerDto;
        }

        ledgerDto.ClientId = wallet.ClientId;
        return ledgerDto;
    }

    protected Result<TResult> HandleDomainException(DomainException ex)
    {
        return Result<TResult>.Failed(ex.Message);
    }

    protected Result<TResult> HandleGenericException(Exception ex, string operation)
    {
        // Log the exception here
        // _logger.LogError(ex, "An unexpected error occurred while {Operation}", operation);

        return Result<TResult>.Failed($"An unexpected error occurred while processing your {operation}. Please try again.");
    }
}