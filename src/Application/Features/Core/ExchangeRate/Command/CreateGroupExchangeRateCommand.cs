using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Create Group Exchange Rate (applies to a client group)
public record CreateGroupExchangeRateCommand(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientGroupId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null) : IRequest<Result<Guid>>;