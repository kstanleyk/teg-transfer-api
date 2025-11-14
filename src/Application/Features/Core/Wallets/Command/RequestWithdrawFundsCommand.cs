using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Command;

public record RequestWithdrawFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Description = null) : IRequest<Result<LedgerDto>>;

public class RequestWithdrawFundsCommandHandler(
    IWalletRepository walletRepository, IClientRepository clientRepository,
    IMapper mapper) : BaseWalletCommandHandler<LedgerDto>(walletRepository, clientRepository), IRequestHandler<RequestWithdrawFundsCommand, Result<LedgerDto>>
{
    public async Task<Result<LedgerDto>> Handle(RequestWithdrawFundsCommand command, CancellationToken cancellationToken)
    {
        var validator = new RequestWithdrawFundsCommandValidator();
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

        var result = await WalletRepository.RequestWithdrawFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<LedgerDto>.Failed("An unexpected error occurred while processing your withdrawal. Please try again.");

        var transactionDto = mapper.Map<LedgerDto>(result.Entity);
        return Result<LedgerDto>.Succeeded(transactionDto, "Withdraw funds request completed successfully, pending admin validation");
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        ClientRepository.Dispose();
    }
}