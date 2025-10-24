using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallet.Query;

// Query to check if balance is sufficient for a specific amount
public record CheckBalanceSufficiencyQuery(Guid ClientId, decimal RequiredAmount, string CurrencyCode)
    : IRequest<Result<BalanceSufficiencyDto>>;

public class CheckBalanceSufficiencyQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<CheckBalanceSufficiencyQuery, Result<BalanceSufficiencyDto>>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<BalanceSufficiencyDto>> Handle(CheckBalanceSufficiencyQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);

        if (wallet == null)
            throw new InvalidOperationException($"Wallet not found for client ID: {query.ClientId}");

        var currency = Domain.ValueObjects.Currency.FromCode(query.CurrencyCode);
        var availableBalance = wallet.AvailableBalance.Amount;
        var isSufficient = availableBalance >= query.RequiredAmount;
        var difference = availableBalance - query.RequiredAmount;

        var result = new BalanceSufficiencyDto
        {
            WalletId = wallet.Id,
            AvailableBalance = availableBalance,
            RequiredAmount = query.RequiredAmount,
            CurrencyCode = currency.Code,
            IsSufficient = isSufficient,
            Difference = difference,
            Message = isSufficient
                ? $"Sufficient balance available"
                : $"Insufficient balance. Required: {query.RequiredAmount} {currency.Code}, Available: {availableBalance} {currency.Code}"
        };

        return Result<BalanceSufficiencyDto>.Succeeded(result);
    }
}