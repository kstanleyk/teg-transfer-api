namespace TegWallet.Application.Features.Core.RateLocks.Dtos;

public record RateLockResponse
{
    public Guid RateLockId { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
    public decimal LockedRate { get; init; }
    public DateTime LockedAt { get; init; }
    public DateTime ValidUntil { get; init; }
    public bool IsUsed { get; init; }
    public bool IsValid { get; init; }
    public bool IsExpiringSoon { get; init; }
    public string ExpirationWarning { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
    public TimeSpan TimeRemaining { get; init; }
    public DateTime? UsedAt { get; init; }
}