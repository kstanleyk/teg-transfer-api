namespace TegWallet.Application.Features.Core.Clients.Dto;

public record ClientRegisteredDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public WalletCreatedDto Wallet { get; init; } = null!;
}