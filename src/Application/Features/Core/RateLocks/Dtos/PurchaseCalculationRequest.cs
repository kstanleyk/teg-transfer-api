using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.RateLocks.Dtos;

public record PurchaseCalculationRequest
{
    public Guid ClientId { get; init; }
    public decimal TargetAmount { get; init; }
    public Currency TargetCurrency { get; init; } = null!;
    public decimal ServiceFeePercentage { get; init; } = 0.02m;
    public bool LockRate { get; init; } = false;
    public TimeSpan? RateLockDuration { get; init; }
    public DateTime? AsOfDate { get; init; }
    public Guid? UseRateLockId { get; init; }
    public string Reference { get; init; } = string.Empty;

    public void Validate()
    {
        if (TargetAmount <= 0)
            throw new DomainException("Target amount must be positive");

        if (ServiceFeePercentage < 0 || ServiceFeePercentage > 1)
            throw new DomainException("Service fee percentage must be between 0 and 1");

        if (LockRate && UseRateLockId.HasValue)
            throw new DomainException("Cannot both lock a new rate and use an existing rate lock");

        if (AsOfDate.HasValue && AsOfDate.Value > DateTime.UtcNow.AddMinutes(5))
            throw new DomainException("AsOfDate cannot be in the future");
    }
}