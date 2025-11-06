namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

// Response DTOs
public record ExchangeRateCreatedResponseDto
{
    public Guid ExchangeRateId { get; init; }
    public string? Message { get; init; }
}
