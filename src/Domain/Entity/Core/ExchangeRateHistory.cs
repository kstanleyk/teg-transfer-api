using TegWallet.Domain.Abstractions;

namespace TegWallet.Domain.Entity.Core;

public class ExchangeRateHistory : Entity<Guid>
{
    public Guid ExchangeRateId { get; private init; }

    // Store the actual currency values, not just rates
    public decimal PreviousBaseCurrencyValue { get; private init; }
    public decimal NewBaseCurrencyValue { get; private init; }
    public decimal PreviousTargetCurrencyValue { get; private init; }
    public decimal NewTargetCurrencyValue { get; private init; }
    public decimal PreviousMargin { get; private init; }
    public decimal NewMargin { get; private init; }

    // Calculated rates for reference
    public decimal PreviousMarketRate { get; private init; }
    public decimal NewMarketRate { get; private init; }
    public decimal PreviousEffectiveRate { get; private init; }
    public decimal NewEffectiveRate { get; private init; }

    public DateTime ChangedAt { get; private init; }
    public string ChangedBy { get; private init; }
    public string ChangeReason { get; private init; }
    public string ChangeType { get; private init; }

    // Private constructor for EF Core
    protected ExchangeRateHistory()
    {
        ChangedBy = string.Empty;
        ChangeReason = string.Empty;
        ChangeType = string.Empty;
    }

    public static ExchangeRateHistory Create(
        Guid exchangeRateId,
        decimal previousBaseValue,
        decimal newBaseValue,
        decimal previousTargetValue,
        decimal newTargetValue,
        decimal previousMargin,
        decimal newMargin,
        string changeReason,
        string changedBy,
        string changeType)
    {
        // Calculate rates for historical reference
        var previousMarketRate = previousBaseValue / previousTargetValue;
        var newMarketRate = newBaseValue / newTargetValue;
        var previousEffectiveRate = previousMarketRate * (1 + previousMargin);
        var newEffectiveRate = newMarketRate * (1 + newMargin);

        return new ExchangeRateHistory
        {
            Id = Guid.NewGuid(),
            ExchangeRateId = exchangeRateId,
            PreviousBaseCurrencyValue = previousBaseValue,
            NewBaseCurrencyValue = newBaseValue,
            PreviousTargetCurrencyValue = previousTargetValue,
            NewTargetCurrencyValue = newTargetValue,
            PreviousMargin = previousMargin,
            NewMargin = newMargin,
            PreviousMarketRate = previousMarketRate,
            NewMarketRate = newMarketRate,
            PreviousEffectiveRate = previousEffectiveRate,
            NewEffectiveRate = newEffectiveRate,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy.Trim(),
            ChangeReason = changeReason.Trim(),
            ChangeType = changeType.Trim()
        };
    }

    public static ExchangeRateHistory CreateFromExchangeRate(
        ExchangeRate exchangeRate,
        string changeReason,
        string changedBy,
        string changeType = "UPDATED")
    {
        return Create(
            exchangeRateId: exchangeRate.Id,
            previousBaseValue: exchangeRate.BaseCurrencyValue,
            newBaseValue: exchangeRate.BaseCurrencyValue, // Same for creation/deactivation
            previousTargetValue: exchangeRate.TargetCurrencyValue,
            newTargetValue: exchangeRate.TargetCurrencyValue,
            previousMargin: exchangeRate.Margin,
            newMargin: exchangeRate.Margin,
            changeReason: changeReason,
            changedBy: changedBy,
            changeType: changeType
        );
    }

    public static ExchangeRateHistory CreateForCreation(ExchangeRate exchangeRate, string createdBy)
    {
        return CreateFromExchangeRate(exchangeRate, "Rate created", createdBy, "CREATED");
    }

    public static ExchangeRateHistory CreateForUpdate(
        ExchangeRate exchangeRate,
        decimal previousBaseValue,
        decimal previousTargetValue,
        decimal previousMargin,
        string updatedBy,
        string reason = "Rate updated")
    {
        return Create(
            exchangeRateId: exchangeRate.Id,
            previousBaseValue: previousBaseValue,
            newBaseValue: exchangeRate.BaseCurrencyValue,
            previousTargetValue: previousTargetValue,
            newTargetValue: exchangeRate.TargetCurrencyValue,
            previousMargin: previousMargin,
            newMargin: exchangeRate.Margin,
            changeReason: reason,
            changedBy: updatedBy,
            changeType: "UPDATED"
        );
    }

    public bool HasBaseValueChanged => PreviousBaseCurrencyValue != NewBaseCurrencyValue;
    public bool HasTargetValueChanged => PreviousTargetCurrencyValue != NewTargetCurrencyValue;
    public bool HasMarginChanged => PreviousMargin != NewMargin;

    public decimal BaseValueChange => NewBaseCurrencyValue - PreviousBaseCurrencyValue;
    public decimal TargetValueChange => NewTargetCurrencyValue - PreviousTargetCurrencyValue;
    public decimal MarginChange => NewMargin - PreviousMargin;
    public decimal MarketRateChange => NewMarketRate - PreviousMarketRate;
    public decimal EffectiveRateChange => NewEffectiveRate - PreviousEffectiveRate;
}