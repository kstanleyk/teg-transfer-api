namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record ExchangeRateTierDto
{
    public Guid Id { get; init; }
    public decimal MinAmount { get; init; }
    public decimal MaxAmount { get; init; }
    public decimal Rate { get; init; }
    public decimal Margin { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}