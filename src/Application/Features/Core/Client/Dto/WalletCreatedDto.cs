namespace TegWallet.Application.Features.Core.Client.Dto;

public record WalletCreatedDto
{
    public Guid Id { get; init; }
    public string BaseCurrencyCode { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal AvailableBalance { get; init; }
    public DateTime CreatedAt { get; init; }
}