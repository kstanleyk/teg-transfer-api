using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record DepositFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Reference = null,
    string? Description = null) : IRequest<Result<TransactionDto>>;

public class DepositFundsCommandHandler(
    IWalletRepository walletRepository,
    IClientRepository clientRepository,
    IMapper mapper) : BaseWalletCommandHandler<TransactionDto>(walletRepository, clientRepository), IRequestHandler<DepositFundsCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(DepositFundsCommand command, CancellationToken cancellationToken)
    {
        var validator = new DepositFundsCommandValidator();
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
            return Result<TransactionDto>.Failed(validation.Message);

        var result = await WalletRepository.DepositFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<TransactionDto>.Failed("An unexpected error occurred while processing your deposit. Please try again.");

        var transactionDto = mapper.Map<TransactionDto>(result.Entity);
        return Result<TransactionDto>.Succeeded(transactionDto);
    }
}