using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.RateLocks.Dtos;

public record CalculatePurchaseAmountResponse
{
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public decimal RequiredBaseAmount { get; init; }
    public decimal ServiceFeeAmount { get; init; }
    public decimal TotalBaseAmount { get; init; }
    public decimal EffectiveExchangeRate { get; init; }
    public string ExchangeRateType { get; init; } = string.Empty;
    public string ClientGroupName { get; init; } = string.Empty;
    public DateTime RateValidUntil { get; init; }
    public string RateLockId { get; init; } = string.Empty;
    public bool IsRateLocked { get; init; }
    public bool IsRateExpiringSoon { get; init; }
    public string RateExpirationWarning { get; init; } = string.Empty;
    public DateTime CalculationTimestamp { get; init; }

    // Additional calculated properties for convenience
    public decimal ServiceFeePercentage { get; init; }
    public string FormattedTargetAmount => $"{TargetAmount:N2} {TargetCurrency}";
    public string FormattedRequiredBaseAmount => $"{RequiredBaseAmount:N2} {BaseCurrency}";
    public string FormattedServiceFeeAmount => $"{ServiceFeeAmount:N2} {BaseCurrency}";
    public string FormattedTotalBaseAmount => $"{TotalBaseAmount:N2} {BaseCurrency}";
    public string FormattedExchangeRate => $"1 {BaseCurrency} = {EffectiveExchangeRate:N6} {TargetCurrency}";
    public string CurrencyPair => $"{BaseCurrency}/{TargetCurrency}";
    public TimeSpan RateLockTimeRemaining => RateValidUntil - DateTime.UtcNow;
    public bool IsRateLockValid => IsRateLocked && RateValidUntil > DateTime.UtcNow;

    // Static factory method for creating responses
    public static CalculatePurchaseAmountResponse Create(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal targetAmount,
        decimal requiredBaseAmount,
        decimal serviceFeeAmount,
        decimal effectiveExchangeRate,
        string exchangeRateType,
        string clientGroupName,
        DateTime rateValidUntil,
        string rateLockId = "",
        bool isRateLocked = false,
        bool isRateExpiringSoon = false,
        string rateExpirationWarning = "",
        DateTime? calculationTimestamp = null)
    {
        var totalBaseAmount = requiredBaseAmount + serviceFeeAmount;
        var serviceFeePercentage = requiredBaseAmount > 0 ? serviceFeeAmount / requiredBaseAmount : 0;

        return new CalculatePurchaseAmountResponse
        {
            BaseCurrency = baseCurrency.Code,
            TargetCurrency = targetCurrency.Code,
            TargetAmount = targetAmount,
            RequiredBaseAmount = Math.Round(requiredBaseAmount, 2),
            ServiceFeeAmount = Math.Round(serviceFeeAmount, 2),
            TotalBaseAmount = Math.Round(totalBaseAmount, 2),
            EffectiveExchangeRate = effectiveExchangeRate,
            ExchangeRateType = exchangeRateType,
            ClientGroupName = clientGroupName,
            RateValidUntil = rateValidUntil,
            RateLockId = rateLockId,
            IsRateLocked = isRateLocked,
            IsRateExpiringSoon = isRateExpiringSoon,
            RateExpirationWarning = rateExpirationWarning,
            CalculationTimestamp = calculationTimestamp ?? DateTime.UtcNow,
            ServiceFeePercentage = Math.Round(serviceFeePercentage, 4)
        };
    }

    // Method to create response from exchange rate and calculation
    public static CalculatePurchaseAmountResponse CreateFromExchangeRate(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal targetAmount,
        ExchangeRate exchangeRate,
        decimal serviceFeePercentage,
        string rateLockId = "",
        bool isRateLocked = false,
        RateLock? rateLock = null)
    {
        // Calculate amounts
        decimal requiredBaseAmount = targetAmount / exchangeRate.EffectiveRate;
        decimal serviceFeeAmount = requiredBaseAmount * serviceFeePercentage;

        // Determine rate validity
        var rateValidUntil = exchangeRate.EffectiveTo ?? DateTime.UtcNow.AddHours(24);

        // Rate lock expiration warnings
        bool isRateExpiringSoon = false;
        string rateExpirationWarning = string.Empty;

        if (rateLock != null)
        {
            isRateExpiringSoon = rateLock.IsExpiringSoon(TimeSpan.FromMinutes(30));
            rateExpirationWarning = rateLock.GetExpirationWarning();
        }

        return Create(
            baseCurrency: baseCurrency,
            targetCurrency: targetCurrency,
            targetAmount: targetAmount,
            requiredBaseAmount: requiredBaseAmount,
            serviceFeeAmount: serviceFeeAmount,
            effectiveExchangeRate: exchangeRate.EffectiveRate,
            exchangeRateType: exchangeRate.Type.ToString(),
            clientGroupName: exchangeRate.ClientGroup?.Name ?? "General",
            rateValidUntil: rateValidUntil,
            rateLockId: rateLockId,
            isRateLocked: isRateLocked,
            isRateExpiringSoon: isRateExpiringSoon,
            rateExpirationWarning: rateExpirationWarning
        );
    }

    // Method for same currency (no conversion needed)
    public static CalculatePurchaseAmountResponse CreateSameCurrency(
        Currency currency,
        decimal targetAmount,
        decimal serviceFeePercentage)
    {
        var serviceFeeAmount = targetAmount * serviceFeePercentage;
        var totalAmount = targetAmount + serviceFeeAmount;

        return Create(
            baseCurrency: currency,
            targetCurrency: currency,
            targetAmount: targetAmount,
            requiredBaseAmount: targetAmount,
            serviceFeeAmount: serviceFeeAmount,
            effectiveExchangeRate: 1.0m,
            exchangeRateType: "Same Currency",
            clientGroupName: "N/A",
            rateValidUntil: DateTime.UtcNow.AddHours(24)
        );
    }

    // Validation method
    public bool IsValid()
    {
        return TargetAmount > 0 &&
               RequiredBaseAmount > 0 &&
               TotalBaseAmount > 0 &&
               EffectiveExchangeRate > 0 &&
               !string.IsNullOrEmpty(BaseCurrency) &&
               !string.IsNullOrEmpty(TargetCurrency) &&
               CalculationTimestamp <= DateTime.UtcNow.AddMinutes(5); // Should be recent
    }

    // Helper method to create response from rate lock
    public static CalculatePurchaseAmountResponse CreateFromRateLock(
        RateLock rateLock,
        Currency baseCurrency,
        decimal targetAmount,
        decimal serviceFeePercentage,
        decimal requiredBaseAmount,
        decimal serviceFeeAmount,
        decimal totalBaseAmount)
    {
        var serviceFeePercentageCalculated = requiredBaseAmount > 0 ? serviceFeeAmount / requiredBaseAmount : 0;

        return new CalculatePurchaseAmountResponse
        {
            BaseCurrency = baseCurrency.Code,
            TargetCurrency = rateLock.TargetCurrency.Code,
            TargetAmount = targetAmount,
            RequiredBaseAmount = requiredBaseAmount,
            ServiceFeeAmount = serviceFeeAmount,
            TotalBaseAmount = totalBaseAmount,
            EffectiveExchangeRate = rateLock.LockedRate,
            ExchangeRateType = rateLock.ExchangeRate?.Type.ToString() ?? "Locked",
            ClientGroupName = rateLock.ExchangeRate?.ClientGroup?.Name ?? "N/A",
            RateValidUntil = rateLock.ValidUntil,
            RateLockId = rateLock.Id.ToString(),
            IsRateLocked = true,
            IsRateExpiringSoon = rateLock.IsExpiringSoon(TimeSpan.FromMinutes(30)), // Default threshold
            RateExpirationWarning = rateLock.GetExpirationWarning(),
            CalculationTimestamp = DateTime.UtcNow,
            ServiceFeePercentage = Math.Round(serviceFeePercentageCalculated, 4)
        };
    }

    // Method to validate the response specifically for rate lock usage
    public bool IsValidForRateLock()
    {
        return IsValid() &&
               IsRateLocked &&
               !string.IsNullOrEmpty(RateLockId) &&
               RateValidUntil > DateTime.UtcNow;
    }

    // Method to get breakdown for display
    public object GetBreakdown()
    {
        return new
        {
            TargetAmount = new { Amount = TargetAmount, Currency = TargetCurrency },
            RequiredBaseAmount = new { Amount = RequiredBaseAmount, Currency = BaseCurrency },
            ServiceFee = new { Amount = ServiceFeeAmount, Currency = BaseCurrency, Percentage = ServiceFeePercentage },
            TotalAmount = new { Amount = TotalBaseAmount, Currency = BaseCurrency },
            ExchangeRate = new { Rate = EffectiveExchangeRate, From = BaseCurrency, To = TargetCurrency },
            RateLock = new { IsLocked = IsRateLocked, LockId = RateLockId, ValidUntil = RateValidUntil }
        };
    }
}