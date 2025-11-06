using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.RateLocks.Dtos;

public record BulkPurchaseCalculationRequest
{
    public Guid ClientId { get; init; }
    public IReadOnlyList<PurchaseCalculationRequest> Items { get; init; } = new List<PurchaseCalculationRequest>();
    public decimal ServiceFeePercentage { get; init; } = 0.02m;
    public bool LockRates { get; init; } = false;
    public TimeSpan? RateLockDuration { get; init; }

    public void Validate()
    {
        if (!Items.Any())
            throw new DomainException("No items provided for calculation");

        if (Items.Count > 50) // Reasonable limit
            throw new DomainException("Cannot process more than 50 items in bulk");

        foreach (var item in Items)
        {
            item.Validate();
        }
    }
}