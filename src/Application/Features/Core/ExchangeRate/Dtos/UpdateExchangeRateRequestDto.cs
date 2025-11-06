namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public record UpdateExchangeRateRequestDto(
    decimal NewBaseCurrencyValue,
    decimal NewTargetCurrencyValue,
    decimal NewMargin,
    string UpdatedBy,
    string Reason = "Rate updated");