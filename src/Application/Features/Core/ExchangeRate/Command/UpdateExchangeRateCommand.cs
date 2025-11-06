using MediatR;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Update Exchange Rate (any type)
public record UpdateExchangeRateCommand(
    Guid ExchangeRateId,
    decimal NewBaseCurrencyValue,
    decimal NewTargetCurrencyValue,
    decimal NewMargin,
    string UpdatedBy,
    string Reason = "Rate updated") : IRequest<Result>;