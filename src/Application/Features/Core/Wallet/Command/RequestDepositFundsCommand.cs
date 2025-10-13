using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record RequestDepositFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Reference = null,
    string? Description = null) : IRequest<Result<TransactionDto>>;

public class RequestDepositFundsCommandHandler(
    IWalletRepository walletRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    IMapper mapper) : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<RequestDepositFundsCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(RequestDepositFundsCommand command, CancellationToken cancellationToken)
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
            return Result<TransactionDto>.Failed(walletValidation.Message);

        var (_, wallet) = walletValidation.Data;

        var currencyValidation = ValidateCurrencyAsync(wallet, command.CurrencyCode);
        if (!currencyValidation.Success)
            return Result<TransactionDto>.Failed(walletValidation.Message);

        var result = await WalletRepository.RequestDepositFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<TransactionDto>.Failed("An unexpected error occurred while processing your deposit. Please try again.");

        var message = localizer["OrderCreatedSuccess"];

        var transactionDto = mapper.Map<TransactionDto>(result.Entity);
        return Result<TransactionDto>.Succeeded(transactionDto,message);
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        ClientRepository.Dispose();
    }
}