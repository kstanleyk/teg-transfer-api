using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Create Individual Exchange Rate (applies to a specific client)
public record CreateIndividualExchangeRateCommand(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Manual",
    DateTime? EffectiveTo = null) : IRequest<Result<Guid>>;