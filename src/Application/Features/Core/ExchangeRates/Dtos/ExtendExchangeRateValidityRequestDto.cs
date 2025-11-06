namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record ExtendExchangeRateValidityRequestDto(
    DateTime NewEffectiveTo,
    string UpdatedBy,
    string Reason = "Validity extended");