using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class ExchangeRate : Entity<Guid>
{
    public Currency BaseCurrency { get; private init; }
    public Currency TargetCurrency { get; private init; }

    // Values representing how many units of each currency equal 1 USD
    // Example: BaseCurrencyValue = 575.50 means 1 USD = 575.50 XOF
    // Example: TargetCurrencyValue = 7.23 means 1 USD = 7.23 CNY
    public decimal BaseCurrencyValue { get; private set; }
    public decimal TargetCurrencyValue { get; private set; }
    public decimal Margin { get; private set; }

    // Calculated properties (not stored in DB)
    public decimal MarketRate => TargetCurrencyValue / BaseCurrencyValue;
    public decimal EffectiveRate => MarketRate * (1 + Margin);

    // Inverse rates (target to base)
    public decimal InverseMarketRate => BaseCurrencyValue / TargetCurrencyValue;
    public decimal InverseEffectiveRate => InverseMarketRate * (1 + Margin);

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

    private readonly List<ExchangeRateTier> _tiers = [];
    public IReadOnlyList<ExchangeRateTier> Tiers => _tiers.AsReadOnly();

    // Method to get applicable tier for an amount
    public ExchangeRateTier? GetApplicableTier(decimal amount)
    {
        return _tiers
            .Where(t => amount >= t.MinAmount && amount <= t.MaxAmount)
            .OrderBy(t => t.MinAmount)
            .FirstOrDefault();
    }

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
        decimal baseCurrencyValue,  // 1 USD = baseCurrencyValue units of base currency
        decimal targetCurrencyValue, // 1 USD = targetCurrencyValue units of target currency
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
        decimal baseCurrencyValue,  // 1 USD = baseCurrencyValue units of base currency
        decimal targetCurrencyValue, // 1 USD = targetCurrencyValue units of target currency
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
        decimal baseCurrencyValue,  // 1 USD = baseCurrencyValue units of base currency
        decimal targetCurrencyValue, // 1 USD = targetCurrencyValue units of target currency
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

    // Add this method to ExchangeRate entity
    public void ApplyTier(ExchangeRateTier tier)
    {
        Margin = tier.Margin;
    }

    public void Expire(DateTime newRateEffectiveFrom, string deactivatedBy, string reason)
    {
        // Set the expiration to 1 second before the new rate starts
        EffectiveTo = newRateEffectiveFrom.AddSeconds(-1);

        // If the new rate starts in the future, we remain active until then
        // If the new rate starts now or in the past, we deactivate immediately
        if (EffectiveTo < DateTime.UtcNow)
        {
            IsActive = false;
        }
    }

    public void MarkAsHistorical()
    {
        IsActive = false;
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

    // Domain methods for managing tiers
    public void AddTier(decimal minAmount, decimal maxAmount, decimal rate, decimal margin, string createdBy)
    {
        var tier = ExchangeRateTier.Create(Id, minAmount, maxAmount,  margin, createdBy);
        _tiers.Add(tier);
    }

    public void RemoveTier(Guid tierId)
    {
        var tier = _tiers.FirstOrDefault(t => t.Id == tierId);
        if (tier != null)
        {
            _tiers.Remove(tier);
        }
    }

    public void ClearTiers()
    {
        _tiers.Clear();
    }

    #region Rate Descriptions

    /// <summary>
    /// Gets the rate description expressing base currency in terms of target currency
    /// Example: "1 XOF = 0.0017 USD (Market: 0.0016, Margin: 6.25%)"
    /// </summary>
    public string GetRateDescription()
    {
        return $"1 {BaseCurrency.Code} = {EffectiveRate:N6} {TargetCurrency.Code} (Market: {MarketRate:N6}, Margin: {Margin:P2})";
    }

    public string GetRateShortDescription()
    {
        return $"1 {BaseCurrency.Code} = {EffectiveRate:N6} {TargetCurrency.Code}";
    }

    /// <summary>
    /// Gets the inverse rate description expressing target currency in terms of base currency
    /// Example: "1 USD = 588.24 XOF (Market: 625.00, Margin: 6.25%)"
    /// </summary>
    public string GetInverseRateDescription()
    {
        return $"1 {TargetCurrency.Code} = {InverseEffectiveRate:N6} {BaseCurrency.Code} (Market: {InverseMarketRate:N6}, Margin: {Margin:P2})";
    }

    public string GetInverseRateShortDescription()
    {
        return $"1 {TargetCurrency.Code} = {InverseEffectiveRate:N6} {BaseCurrency.Code}";
    }

    /// <summary>
    /// Gets both rate descriptions for comprehensive display
    /// </summary>
    public string GetDualRateDescription()
    {
        return $"{GetRateDescription()}{Environment.NewLine}{GetInverseRateDescription()}";
    }

    /// <summary>
    /// Gets the appropriate rate description based on the conversion direction
    /// </summary>
    public string GetDirectionalRateDescription(Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency == BaseCurrency && toCurrency == TargetCurrency)
            return GetRateDescription();
        else if (fromCurrency == TargetCurrency && toCurrency == BaseCurrency)
            return GetInverseRateDescription();
        else
            throw new DomainException($"Invalid currency pair for this exchange rate: {fromCurrency.Code} to {toCurrency.Code}");
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Converts an amount from base currency to target currency
    /// Example: Convert 1000 XOF to USD using XOF→USD rate
    /// </summary>
    public decimal ConvertToTarget(decimal baseAmount)
    {
        if (baseAmount < 0)
            throw new DomainException("Amount to convert must be non-negative");

        return baseAmount * EffectiveRate;
    }

    /// <summary>
    /// Converts an amount from target currency to base currency
    /// Example: Convert 100 USD to XOF using XOF→USD rate
    /// </summary>
    public decimal ConvertToBase(decimal targetAmount)
    {
        if (targetAmount < 0)
            throw new DomainException("Amount to convert must be non-negative");

        return targetAmount / EffectiveRate;
    }

    /// <summary>
    /// Converts an amount from target currency to base currency using inverse rate
    /// This is equivalent to ConvertToBase but uses the inverse calculation
    /// Example: Convert 100 USD to XOF using USD→XOF inverse rate
    /// </summary>
    public decimal ConvertUsingInverseRate(decimal targetAmount)
    {
        if (targetAmount < 0)
            throw new DomainException("Amount to convert must be non-negative");

        return targetAmount * InverseEffectiveRate;
    }

    /// <summary>
    /// Gets the conversion rate between any two currencies (handles both directions)
    /// </summary>
    public decimal GetConversionRate(Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency == BaseCurrency && toCurrency == TargetCurrency)
            return EffectiveRate;
        else if (fromCurrency == TargetCurrency && toCurrency == BaseCurrency)
            return InverseEffectiveRate;
        else
            throw new DomainException($"Cannot convert from {fromCurrency.Code} to {toCurrency.Code} with this exchange rate");
    }

    /// <summary>
    /// Converts between any two currencies using the appropriate rate direction
    /// </summary>
    public decimal ConvertAmount(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (amount < 0)
            throw new DomainException("Amount to convert must be non-negative");

        if (fromCurrency == BaseCurrency && toCurrency == TargetCurrency)
            return ConvertToTarget(amount);
        else if (fromCurrency == TargetCurrency && toCurrency == BaseCurrency)
            return ConvertToBase(amount);
        else
            throw new DomainException($"Cannot convert from {fromCurrency.Code} to {toCurrency.Code} with this exchange rate");
    }

    #endregion

    #region Validation and Utility Methods

    /// <summary>
    /// Validates if this exchange rate can be used for a given amount and currency pair
    /// </summary>
    public bool CanConvert(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (!IsActive) return false;
        if (!IsEffectiveAt(DateTime.UtcNow)) return false;
        if (amount < 0) return false;

        return (fromCurrency == BaseCurrency && toCurrency == TargetCurrency) ||
               (fromCurrency == TargetCurrency && toCurrency == BaseCurrency);
    }

    /// <summary>
    /// Gets a summary of this exchange rate including both directions
    /// </summary>
    public string GetRateSummary()
    {
        var rateTypeDescription = Type switch
        {
            RateType.General => "General Rate",
            RateType.Group => "Group Rate",
            RateType.Individual => "Individual Rate",
            _ => "Unknown Rate Type"
        };

        var clientInfo = Type switch
        {
            RateType.Individual when ClientId.HasValue => $" (Client: {ClientId})",
            RateType.Group when ClientGroupId.HasValue => $" (Group: {ClientGroupId})",
            _ => ""
        };

        return $"{rateTypeDescription}{clientInfo}: {GetDualRateDescription()}";
    }

    /// <summary>
    /// Calculates the margin amount for a given conversion
    /// </summary>
    public decimal CalculateMarginAmount(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (!CanConvert(amount, fromCurrency, toCurrency))
            throw new DomainException("Cannot calculate margin for invalid conversion");

        var marketRate = fromCurrency == BaseCurrency ? MarketRate : InverseMarketRate;
        var effectiveRate = fromCurrency == BaseCurrency ? EffectiveRate : InverseEffectiveRate;

        var amountWithoutMargin = amount * marketRate;
        var amountWithMargin = amount * effectiveRate;

        return amountWithMargin - amountWithoutMargin;
    }

    // Validation methods
    private static void ValidateCurrencyValues(decimal baseCurrencyValue, decimal targetCurrencyValue)
    {
        if (baseCurrencyValue <= 0)
            throw new DomainException("Base currency value must be positive (1 USD = X base currency units)");

        if (targetCurrencyValue <= 0)
            throw new DomainException("Target currency value must be positive (1 USD = X target currency units)");

        // Additional validation to prevent unrealistic rates
        if (baseCurrencyValue > 1000000)
            throw new DomainException("Base currency value appears to be unrealistic");

        if (targetCurrencyValue > 1000000)
            throw new DomainException("Target currency value appears to be unrealistic");

        // Validate that rates are not too close to zero to avoid division issues
        if (baseCurrencyValue < 0.000001m)
            throw new DomainException("Base currency value is too small");

        if (targetCurrencyValue < 0.000001m)
            throw new DomainException("Target currency value is too small");
    }

    private static void ValidateMargin(decimal margin)
    {
        if (margin < 0 || margin > 1)
            throw new DomainException("Margin must be between 0 and 1 (0% to 100%)");

        // Warn about very high margins (optional)
        if (margin > 0.5m) // 50% margin
        {
            // You could log a warning here, but not throw an exception
        }
    }

    private static void ValidateEffectiveDate(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        //if (effectiveFrom < DateTime.UtcNow.AddMinutes(-5))
        //    throw new DomainException("Effective date cannot be more than 5 minutes in the past");

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            throw new DomainException("End date must be after start date");

        if (effectiveTo.HasValue && effectiveTo.Value > DateTime.UtcNow.AddYears(10))
            throw new DomainException("End date cannot be more than 10 years in the future");
    }

    #endregion
}

public enum RateType
{
    General = 1,    // Applies to all clients
    Group = 2,      // Applies to a client group
    Individual = 3  // Applies to a specific client
}
