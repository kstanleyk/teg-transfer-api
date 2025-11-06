namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record CreateIndividualExchangeRateRequestDto(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Manual",
    DateTime? EffectiveTo = null);