namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public record DeactivateExchangeRateRequestDto(
    string DeactivatedBy,
    string Reason = "Rate deactivated");