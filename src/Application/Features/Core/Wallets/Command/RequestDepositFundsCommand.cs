using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
    UserManager<Domain.Entity.Core.Client> userManager,
    IAppLocalizer localizer,
    IMapper mapper) : BaseWalletCommandHandler<LedgerDto>(walletRepository, userManager), IRequestHandler<RequestDepositFundsCommand, Result<LedgerDto>>
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

        var transactionDto = mapper.Map<LedgerDto>(result.Entity);
        return Result<LedgerDto>.Succeeded(transactionDto,message);
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        UserManager.Dispose();
    }
}