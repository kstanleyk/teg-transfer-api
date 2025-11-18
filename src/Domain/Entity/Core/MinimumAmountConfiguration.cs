using TegWallet.Domain.Abstractions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class MinimumAmountConfiguration: Entity<Guid>
{
    public Currency BaseCurrency { get; private set; }
    public Currency TargetCurrency { get; private set; }
    public decimal MinimumAmount { get; private set; }  // In TARGET currency
    public bool IsActive { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static MinimumAmountConfiguration Create(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal minimumAmount,
        DateTime effectiveFrom,
        string createdBy,
        DateTime? effectiveTo = null)
    {
        return new MinimumAmountConfiguration
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            MinimumAmount = minimumAmount,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(decimal newMinimumAmount, DateTime newEffectiveFrom, DateTime? newEffectiveTo)
    {
        MinimumAmount = newMinimumAmount;
        EffectiveFrom = newEffectiveFrom;
        EffectiveTo = newEffectiveTo;
    }

    public void Deactivate() => IsActive = false;
}