namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record UpdateExchangeRateRequestDto(
    decimal NewBaseCurrencyValue,
    decimal NewTargetCurrencyValue,
    decimal NewMargin,
    string UpdatedBy,
    string Reason = "Rate updated");