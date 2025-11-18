namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

//public record CreateGeneralExchangeRateRequestDto(
//    string BaseCurrencyCode,
//    string TargetCurrencyCode,
//    decimal BaseCurrencyValue,
//    decimal TargetCurrencyValue,
//    decimal Margin,
//    DateTime EffectiveFrom,
//    string CreatedBy = "SYSTEM",
//    string Source = "Market",
//    DateTime? EffectiveTo = null);

public record CreateGeneralExchangeRateRequestDto(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null,
    List<ExchangeRateTierRequestDto>? Tiers = null); // NEW: Optional tiers