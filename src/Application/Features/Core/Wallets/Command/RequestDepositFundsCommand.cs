using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;

namespace TegWallet.Application.Features.Core.Wallets.Command;

public record RequestDepositFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Reference = null,
    string? Description = null) : IRequest<Result<LedgerDto>>;

public class RequestDepositFundsCommandHandler(
    IWalletRepository walletRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    IMapper mapper) : BaseWalletCommandHandler<LedgerDto>(walletRepository, clientRepository), IRequestHandler<RequestDepositFundsCommand, Result<LedgerDto>>
{
    public async Task<Result<LedgerDto>> Handle(RequestDepositFundsCommand command, CancellationToken cancellationToken)
    {
        var validator = new RequestDepositFundsCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        var walletValidation = await ValidateClientAndWalletAsync(command.ClientId);
        if (!walletValidation.Success)
            return Result<LedgerDto>.Failed(walletValidation.Message);

        var (_, wallet) = walletValidation.Data;

        var currencyValidation = ValidateCurrencyAsync(wallet, command.CurrencyCode);
        if (!currencyValidation.Success)
            return Result<LedgerDto>.Failed(walletValidation.Message);

        var result = await WalletRepository.RequestDepositFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<LedgerDto>.Failed("An unexpected error occurred while processing your deposit. Please try again.");

        var message = localizer["OrderCreatedSuccess"];

        var ledgerDto = mapper.Map<LedgerDto>(result.Entity);

        await UpdateClient(ledgerDto, result.Entity!.WalletId);

        return Result<LedgerDto>.Succeeded(ledgerDto, message);
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        ClientRepository.Dispose();
    }
}