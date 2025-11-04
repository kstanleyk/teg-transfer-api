using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class ExchangeRate : Entity<Guid>
{
    public Currency BaseCurrency { get; private init; }
    public Currency TargetCurrency { get; private init; }

    // Values in a common reference currency (like USD)
    public decimal BaseCurrencyValue { get; private set; }  // Value of 1 unit of base currency in reference currency
    public decimal TargetCurrencyValue { get; private set; } // Value of 1 unit of target currency in reference currency
    public decimal Margin { get; private set; } // Bank's margin/commission

    // Calculated properties
    public decimal MarketRate => CalculateMarketRate();
    public decimal EffectiveRate => CalculateEffectiveRate();

    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public RateType Type { get; private init; }
    public string? ClientGroup { get; private set; }
    public Guid? ClientId { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public string Source { get; private set; }
    public string CreatedBy { get; private set; }

    // Private constructor for EF Core
    protected ExchangeRate()
    {
        Source = string.Empty;
        CreatedBy = string.Empty;
    }

    // Factory methods for different rate types

    public static ExchangeRate CreateGeneralRate(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal baseCurrencyValue,
        decimal targetCurrencyValue,
        decimal margin,
        DateTime effectiveFrom,
        string createdBy = "SYSTEM",
        string source = "Market",
        DateTime? effectiveTo = null)
    {
        ValidateCurrencyValues(baseCurrencyValue, targetCurrencyValue);
        ValidateMargin(margin);
        ValidateEffectiveDate(effectiveFrom, effectiveTo);

        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            BaseCurrencyValue = baseCurrencyValue,
            TargetCurrencyValue = targetCurrencyValue,
            Margin = margin,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            Type = RateType.General,
            Source = source.Trim(),
            CreatedBy = createdBy.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ExchangeRate CreateGroupRate(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal baseCurrencyValue,
        decimal targetCurrencyValue,
        decimal margin,
        string clientGroup,
        DateTime effectiveFrom,
        string createdBy = "SYSTEM",
        string source = "Market",
        DateTime? effectiveTo = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(clientGroup, nameof(clientGroup));
        ValidateCurrencyValues(baseCurrencyValue, targetCurrencyValue);
        ValidateMargin(margin);
        ValidateEffectiveDate(effectiveFrom, effectiveTo);

        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            BaseCurrencyValue = baseCurrencyValue,
            TargetCurrencyValue = targetCurrencyValue,
            Margin = margin,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            Type = RateType.Group,
            ClientGroup = clientGroup.Trim(),
            Source = source.Trim(),
            CreatedBy = createdBy.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ExchangeRate CreateIndividualRate(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal baseCurrencyValue,
        decimal targetCurrencyValue,
        decimal margin,
        Guid clientId,
        DateTime effectiveFrom,
        string createdBy = "SYSTEM",
        string source = "Manual",
        DateTime? effectiveTo = null)
    {
        DomainGuards.AgainstDefault(clientId, nameof(clientId));
        ValidateCurrencyValues(baseCurrencyValue, targetCurrencyValue);
        ValidateMargin(margin);
        ValidateEffectiveDate(effectiveFrom, effectiveTo);

        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            BaseCurrencyValue = baseCurrencyValue,
            TargetCurrencyValue = targetCurrencyValue,
            Margin = margin,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            Type = RateType.Individual,
            ClientId = clientId,
            Source = source.Trim(),
            CreatedBy = createdBy.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateCurrencyValues(decimal newBaseCurrencyValue, decimal newTargetCurrencyValue, decimal newMargin)
    {
        ValidateCurrencyValues(newBaseCurrencyValue, newTargetCurrencyValue);
        ValidateMargin(newMargin);

        BaseCurrencyValue = newBaseCurrencyValue;
        TargetCurrencyValue = newTargetCurrencyValue;
        Margin = newMargin;
    }

    public void Deactivate()
    {
        IsActive = false;
        EffectiveTo = DateTime.UtcNow;
    }

    public void ExtendValidity(DateTime newEffectiveTo)
    {
        if (newEffectiveTo <= EffectiveFrom)
            throw new DomainException("New effective date must be after start date");

        EffectiveTo = newEffectiveTo;
    }

    public bool IsEffectiveAt(DateTime date)
    {
        return IsActive &&
               date >= EffectiveFrom &&
               (EffectiveTo == null || date <= EffectiveTo.Value);
    }

    public decimal ConvertToTarget(decimal baseAmount)
    {
        return baseAmount * EffectiveRate;
    }

    public decimal ConvertToBase(decimal targetAmount)
    {
        return targetAmount / EffectiveRate;
    }

    // Private calculation methods
    private decimal CalculateMarketRate()
    {
        // Market rate: How much target currency for 1 base currency
        // If 1 BaseCurrency = BaseCurrencyValue USD and 1 TargetCurrency = TargetCurrencyValue USD
        // Then 1 BaseCurrency = (BaseCurrencyValue / TargetCurrencyValue) TargetCurrency
        return BaseCurrencyValue / TargetCurrencyValue;
    }

    private decimal CalculateEffectiveRate()
    {
        // Effective rate = Market rate with bank's margin added
        return MarketRate * (1 + Margin);
    }

    // Validation methods
    private static void ValidateCurrencyValues(decimal baseCurrencyValue, decimal targetCurrencyValue)
    {
        if (baseCurrencyValue <= 0)
            throw new DomainException("Base currency value must be positive");

        if (targetCurrencyValue <= 0)
            throw new DomainException("Target currency value must be positive");
    }

    private static void ValidateMargin(decimal margin)
    {
        if (margin < 0 || margin > 1)
            throw new DomainException("Margin must be between 0 and 1 (0% to 100%)");
    }

    private static void ValidateEffectiveDate(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveFrom < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("Effective date cannot be in the past");

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            throw new DomainException("End date must be after start date");
    }
}

public enum RateType
{
    General = 1,    // Applies to all clients
    Group = 2,      // Applies to a client group
    Individual = 3  // Applies to a specific client
}
