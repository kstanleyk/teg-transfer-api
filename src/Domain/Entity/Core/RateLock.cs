using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class RateLock : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public Guid ExchangeRateId { get; private init; }
    public ExchangeRate ExchangeRate { get; private init; } = null!;
    public Currency BaseCurrency { get; private init; } = null!;
    public Currency TargetCurrency { get; private init; } = null!;
    public decimal LockedRate { get; private init; }
    public DateTime LockedAt { get; private init; }
    public DateTime ValidUntil { get; private set; }
    public bool IsUsed { get; private set; }
    public string LockReference { get; private init; } = string.Empty;
    public DateTime? UsedAt { get; private set; }

    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }

    // Navigation properties
    public Client Client { get; private init; } = null!;

    private RateLock() { }

    public static RateLock Create(
        Guid clientId,
        ExchangeRate exchangeRate,
        TimeSpan lockDuration,
        string lockReference)
    {
        if (lockDuration <= TimeSpan.Zero)
            throw new DomainException("Lock duration must be positive");

        if (exchangeRate.EffectiveTo.HasValue && exchangeRate.EffectiveTo.Value < DateTime.UtcNow.Add(lockDuration))
            throw new DomainException("Cannot lock rate beyond its effective period");

        return new RateLock
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ExchangeRateId = exchangeRate.Id,
            BaseCurrency = exchangeRate.BaseCurrency,
            TargetCurrency = exchangeRate.TargetCurrency,
            LockedRate = exchangeRate.EffectiveRate,
            LockedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.Add(lockDuration),
            IsUsed = false,
            LockReference = lockReference
        };
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Rate lock has already been used");

        if (!IsValid())
            throw new DomainException("Rate lock is no longer valid");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled(string reason, string cancelledBy = "SYSTEM")
    {
        if (IsUsed)
            throw new DomainException("Rate lock has already been used");

        if (!IsValid())
            throw new DomainException("Cannot cancel an expired rate lock");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        CancellationReason = reason.Trim();
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy.Trim();
    }

    public string GetExpirationWarning()
    {
        // If the rate lock is already used or expired, return appropriate message
        if (IsUsed)
        {
            return UsedAt.HasValue
                ? $"Rate lock was used on {UsedAt.Value:yyyy-MM-dd HH:mm:ss}"
                : "Rate lock has been used";
        }

        if (!IsValid())
        {
            return "Rate lock has expired";
        }

        var timeUntilExpiry = ValidUntil - DateTime.UtcNow;

        // Handle edge case where time is exactly at or past expiry
        if (timeUntilExpiry <= TimeSpan.Zero)
        {
            return "Rate lock has expired";
        }

        // Return appropriate warning message based on time remaining
        return timeUntilExpiry switch
        {
            // Less than 1 minute
            var t when t <= TimeSpan.FromMinutes(1) =>
                $"Rate lock expires in {timeUntilExpiry.Seconds} second{(timeUntilExpiry.Seconds == 1 ? "" : "s")}",

            // Less than 5 minutes
            var t when t <= TimeSpan.FromMinutes(5) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")} and {timeUntilExpiry.Seconds} second{(timeUntilExpiry.Seconds == 1 ? "" : "s")}",

            // Less than 30 minutes
            var t when t <= TimeSpan.FromMinutes(30) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            // Less than 1 hour
            var t when t <= TimeSpan.FromHours(1) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            // Less than 2 hours
            var t when t <= TimeSpan.FromHours(2) =>
                $"Rate lock expires in {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")} and {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            // Less than 24 hours
            var t when t <= TimeSpan.FromHours(24) =>
                $"Rate lock expires in {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")}",

            // 1-2 days
            var t when t <= TimeSpan.FromDays(2) =>
                $"Rate lock expires in {timeUntilExpiry:%d} day{(timeUntilExpiry.Days == 1 ? "" : "s")} and {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")}",

            // More than 2 days
            _ => $"Rate lock expires in {timeUntilExpiry:%d} day{(timeUntilExpiry.Days == 1 ? "" : "s")}"
        };
    }

    // Alternative implementation with configurable warning thresholds
    public string GetExpirationWarning(TimeSpan warningThreshold)
    {
        if (IsUsed)
        {
            return UsedAt.HasValue
                ? $"Rate lock was used on {UsedAt.Value:yyyy-MM-dd HH:mm:ss}"
                : "Rate lock has been used";
        }

        if (!IsValid())
        {
            return "Rate lock has expired";
        }

        var timeUntilExpiry = ValidUntil - DateTime.UtcNow;

        if (timeUntilExpiry <= TimeSpan.Zero)
        {
            return "Rate lock has expired";
        }

        // Check if it's expiring soon based on the provided threshold
        bool isExpiringSoon = timeUntilExpiry <= warningThreshold;

        if (!isExpiringSoon)
        {
            // Return generic message for non-urgent cases
            if (timeUntilExpiry <= TimeSpan.FromHours(24))
            {
                return $"Rate lock expires in {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")}";
            }
            else
            {
                return $"Rate lock expires in {timeUntilExpiry:%d} day{(timeUntilExpiry.Days == 1 ? "" : "s")}";
            }
        }

        // Detailed warning for expiring soon cases
        return timeUntilExpiry switch
        {
            var t when t <= TimeSpan.FromMinutes(1) =>
                "Rate lock expires in less than a minute!",

            var t when t <= TimeSpan.FromMinutes(5) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            var t when t <= TimeSpan.FromMinutes(30) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            var t when t <= TimeSpan.FromHours(1) =>
                $"Rate lock expires in {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            var t when t <= TimeSpan.FromHours(2) =>
                $"Rate lock expires in {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")} and {timeUntilExpiry.Minutes} minute{(timeUntilExpiry.Minutes == 1 ? "" : "s")}",

            _ => $"Rate lock expires in {timeUntilExpiry:%h} hour{(timeUntilExpiry.Hours == 1 ? "" : "s")}"
        };
    }

    // Helper method to get detailed status with time information
    public string GetDetailedStatus()
    {
        var status = GetStatusDescription();
        if (status == "Active")
        {
            return $"{status} - {GetExpirationWarning()}";
        }
        return status;
    }

    public void Extend(TimeSpan additionalDuration)
    {
        if (IsUsed)
            throw new DomainException("Cannot extend a used rate lock");

        if (additionalDuration <= TimeSpan.Zero)
            throw new DomainException("Additional duration must be positive");

        if (!IsValid())
            throw new DomainException("Cannot extend an expired or invalid rate lock");

        // Check if we're not extending beyond any maximum limits
        // This could be based on business rules - for example, maximum total lock duration
        var totalDuration = (ValidUntil - LockedAt) + additionalDuration;
        var maximumTotalDuration = TimeSpan.FromDays(30); // Example: 30 days maximum

        if (totalDuration > maximumTotalDuration)
        {
            throw new DomainException($"Total rate lock duration cannot exceed {maximumTotalDuration.TotalDays} days");
        }

        ValidUntil = ValidUntil.Add(additionalDuration);
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow <= ValidUntil;
    }

    public bool IsExpiringSoon(TimeSpan threshold)
    {
        if (!IsValid()) return false;

        var timeUntilExpiry = ValidUntil - DateTime.UtcNow;
        return timeUntilExpiry <= threshold;
    }

    // Helper method to check if rate lock was cancelled (as opposed to used normally)
    public bool IsCancelled => IsUsed && !string.IsNullOrEmpty(CancellationReason);

    // Update GetExpirationWarning to consider cancellation
    public string GetStatusDescription()
    {
        if (IsCancelled) return $"Cancelled: {CancellationReason}";
        if (IsUsed) return "Used";
        if (!IsValid()) return "Expired";
        if (IsExpiringSoon(TimeSpan.FromMinutes(30))) return "Expiring Soon";
        return "Active";
    }

    // Additional helper methods
    public string CurrencyPair => $"{BaseCurrency.Code}/{TargetCurrency.Code}";

    public bool IsForCurrencyPair(Currency baseCurrency, Currency targetCurrency)
    {
        return BaseCurrency == baseCurrency && TargetCurrency == targetCurrency;
    }

    public decimal CalculateRequiredAmount(decimal targetAmount)
    {
        return targetAmount / LockedRate;
    }

    public decimal CalculateTargetAmount(decimal baseAmount)
    {
        return baseAmount * LockedRate;
    }
}

public class RateLockingSettings
{
    public bool Enabled { get; set; } = true;
    public TimeSpan DefaultLockDuration { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan MaxLockDuration { get; set; } = TimeSpan.FromHours(72);
    public decimal MaximumLockAmount { get; set; } = 1_000_000;
    public int MaxActiveLocksPerClient { get; set; } = 5;
    public bool AllowLockExtension { get; set; } = true;
    public TimeSpan ExtensionDuration { get; set; } = TimeSpan.FromHours(12);
    public TimeSpan ExpirationWarningThreshold { get; set; } = TimeSpan.FromMinutes(30);
}