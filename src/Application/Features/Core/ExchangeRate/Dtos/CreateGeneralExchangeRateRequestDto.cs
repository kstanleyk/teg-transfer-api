namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public record CreateGeneralExchangeRateRequestDto(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null);