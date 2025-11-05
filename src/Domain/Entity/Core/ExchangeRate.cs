using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

using System.ComponentModel.DataAnnotations.Schema;

public class ExchangeRate : Entity<Guid>
{
    public Currency BaseCurrency { get; private init; }
    public Currency TargetCurrency { get; private init; }

    // Values in a common reference currency (like USD)
    public decimal BaseCurrencyValue { get; private set; }
    public decimal TargetCurrencyValue { get; private set; }
    public decimal Margin { get; private set; }

    // Calculated properties (not stored in DB)
    [NotMapped]
    public decimal MarketRate => BaseCurrencyValue / TargetCurrencyValue;

    [NotMapped]
    public decimal EffectiveRate => MarketRate * (1 + Margin);

    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public RateType Type { get; private init; }

    // Client targeting - now using GUID references
    public Guid? ClientId { get; private init; }
    public Guid? ClientGroupId { get; private init; }

    // Navigation properties
    public Client? Client { get; private set; }
    public ClientGroup? ClientGroup { get; private set; }

    public DateTime CreatedAt { get; private init; }
    public string Source { get; private set; }
    public string CreatedBy { get; private set; }

    // Private constructor for EF Core
    private ExchangeRate()
    {
        Source = string.Empty;
        CreatedBy = string.Empty;
    }

    // Factory methods
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
        Guid clientGroupId,
        DateTime effectiveFrom,
        string createdBy = "SYSTEM",
        string source = "Market",
        DateTime? effectiveTo = null)
    {
        DomainGuards.AgainstDefault(clientGroupId, nameof(clientGroupId));
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
            ClientGroupId = clientGroupId,
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
        if (!IsActive) return;
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

    public decimal ConvertToTarget(decimal baseAmount) => baseAmount * EffectiveRate;
    public decimal ConvertToBase(decimal targetAmount) => targetAmount / EffectiveRate;

    // Validation methods
    private static void ValidateCurrencyValues(decimal baseCurrencyValue, decimal targetCurrencyValue)
    {
        if (baseCurrencyValue <= 0) throw new DomainException("Base currency value must be positive");
        if (targetCurrencyValue <= 0) throw new DomainException("Target currency value must be positive");
    }

    private static void ValidateMargin(decimal margin)
    {
        if (margin < 0 || margin > 1) throw new DomainException("Margin must be between 0 and 1");
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
