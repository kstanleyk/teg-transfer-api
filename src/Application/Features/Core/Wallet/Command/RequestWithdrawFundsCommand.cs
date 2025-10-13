using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record RequestWithdrawFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Description = null) : IRequest<Result<TransactionDto>>;

public class RequestWithdrawFundsCommandHandler(
    IWalletRepository walletRepository, IClientRepository clientRepository,
    IMapper mapper) : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<RequestWithdrawFundsCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(RequestWithdrawFundsCommand command, CancellationToken cancellationToken)
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
            return Result<TransactionDto>.Failed(walletValidation.Message);

        var (_, wallet) = walletValidation.Data;

        var currencyValidation = ValidateCurrencyAsync(wallet, command.CurrencyCode);
        if (!currencyValidation.Success)
            return Result<TransactionDto>.Failed(walletValidation.Message);

        var result = await WalletRepository.RequestWithdrawFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<TransactionDto>.Failed("An unexpected error occurred while processing your withdrawal. Please try again.");

        var transactionDto = mapper.Map<TransactionDto>(result.Entity);
        return Result<TransactionDto>.Succeeded(transactionDto, "Withdraw funds request completed successfully, pending admin validation");
    }

    protected override void DisposeCore()
    {
        WalletRepository.Dispose();
        ClientRepository.Dispose();
    }
}