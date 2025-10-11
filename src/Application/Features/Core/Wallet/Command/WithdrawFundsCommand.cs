using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Command;

public record WithdrawFundsCommand(
    Guid ClientId,
    decimal Amount,
    string CurrencyCode,
    string? Description = null) : IRequest<Result<TransactionDto>>;

public class WithdrawFundsCommandHandler(
    IWalletRepository walletRepository,
    IMapper mapper) : IRequestHandler<WithdrawFundsCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(WithdrawFundsCommand command, CancellationToken cancellationToken)
    {
        var validator = new WithdrawFundsCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        var result = await walletRepository.WithdrawFundsAsync(command);
        if (result.Status != RepositoryActionStatus.Updated)
            return Result<TransactionDto>.Failed("An unexpected error occurred while processing your withdrawal. Please try again.");

        var transactionDto = mapper.Map<TransactionDto>(result.Entity);
        return Result<TransactionDto>.Succeeded(transactionDto);
    }
}