namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record ExchangeRateTierRequestDto(
    decimal MinAmount,
    decimal MaxAmount,
    decimal Rate,
    decimal Margin,
    string CreatedBy = "SYSTEM");