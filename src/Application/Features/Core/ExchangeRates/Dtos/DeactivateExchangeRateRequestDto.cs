namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record DeactivateExchangeRateRequestDto(
    string DeactivatedBy,
    string Reason = "Rate deactivated");