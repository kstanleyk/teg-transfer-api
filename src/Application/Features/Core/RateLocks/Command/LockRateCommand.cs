using MediatR;
using Microsoft.Extensions.Options;
using TegWallet.Application.Features.Core.RateLocks.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.RateLocks.Command;


// Lock a rate for future use
public record LockRateCommand : IRequest<Result<RateLockResponse>>
{
    public Guid ClientId { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
    public TimeSpan? Duration { get; init; }
    public string Reference { get; init; } = string.Empty;
}

public class LockRateCommandHandler(
    IClientRepository clientRepository,
    IExchangeRateRepository exchangeRateRepository,
    IRateLockRepository rateLockRepository,
    IOptions<RateLockingSettings> rateLockingSettings,
    IAppLocalizer localizer)
    : IRequestHandler<LockRateCommand, Result<RateLockResponse>>
{
    public async Task<Result<RateLockResponse>> Handle(LockRateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if rate locking is enabled
            if (!rateLockingSettings.Value.Enabled)
            {
                return Result<RateLockResponse>.Failed(localizer["Rate locking is currently disabled"]);
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(request.BaseCurrency))
                return Result<RateLockResponse>.Failed(localizer["Base currency is required"]);

            if (string.IsNullOrWhiteSpace(request.TargetCurrency))
                return Result<RateLockResponse>.Failed(localizer["Target currency is required"]);

            // Validate currencies
            Currency baseCurrency;
            Currency targetCurrency;

            try
            {
                baseCurrency = Currency.FromCode(request.BaseCurrency);
            }
            catch (Exception)
            {
                return Result<RateLockResponse>.Failed(localizer["Invalid base currency"]);
            }

            try
            {
                targetCurrency = Currency.FromCode(request.TargetCurrency);
            }
            catch (Exception)
            {
                return Result<RateLockResponse>.Failed(localizer["Invalid target currency"]);
            }

            // Validate duration
            var duration = request.Duration ?? rateLockingSettings.Value.DefaultLockDuration;

            if (duration <= TimeSpan.Zero)
                return Result<RateLockResponse>.Failed(localizer["Lock duration must be positive"]);

            if (duration > rateLockingSettings.Value.MaxLockDuration)
                return Result<RateLockResponse>.Failed(localizer["Lock duration exceeds maximum allowed"]);

            // Get client
            var client = await clientRepository.GetAsync(request.ClientId);
            if (client == null)
                return Result<RateLockResponse>.Failed(localizer["Client not found"]);

            // Check rate lock availability
            var availabilityCheck = await CheckRateLockAvailabilityAsync(request.ClientId, baseCurrency, targetCurrency);
            if (!availabilityCheck.CanCreateNewLock)
                return Result<RateLockResponse>.Failed(availabilityCheck.Reason ?? "Cannot create new rate lock");

            // Get applicable exchange rate
            var exchangeRate = await exchangeRateRepository.GetApplicableRateAsync(
                request.ClientId,
                client.ClientGroupId,
                baseCurrency,
                targetCurrency,
                DateTime.UtcNow);

            if (exchangeRate == null)
                return Result<RateLockResponse>.Failed(localizer[$"No exchange rate available for {baseCurrency.Code} to {targetCurrency.Code}"]);

            // Validate exchange rate can be locked for the requested duration
            if (exchangeRate.EffectiveTo.HasValue && exchangeRate.EffectiveTo.Value < DateTime.UtcNow.Add(duration))
                return Result<RateLockResponse>.Failed(localizer["Cannot lock rate beyond its effective period"]);

            // Create rate lock
            var rateLock = RateLock.Create(
                request.ClientId,
                exchangeRate,
                duration,
                request.Reference);

            // Save rate lock
            await rateLockRepository.AddAsync(rateLock);

            // Build response
            var response = BuildRateLockResponse(rateLock);

            return Result<RateLockResponse>.Succeeded(response);
        }
        catch (DomainException ex)
        {
            return Result<RateLockResponse>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<RateLockResponse>.Failed(localizer["An unexpected error occurred while locking the rate"]);
        }
    }

    private async Task<RateLockAvailability> CheckRateLockAvailabilityAsync(Guid clientId, Currency baseCurrency, Currency targetCurrency)
    {
        // Get active rate locks for the client
        var activeLocks = await rateLockRepository.GetClientRateLocksAsync(clientId);
        var currentCount = activeLocks.Count(rl => rl.IsValid() && !rl.IsUsed);

        // Check maximum active locks limit
        if (currentCount >= rateLockingSettings.Value.MaxActiveLocksPerClient)
        {
            return RateLockAvailability.Unavailable(
                $"Maximum number of active rate locks ({rateLockingSettings.Value.MaxActiveLocksPerClient}) reached");
        }

        // Check if there's already an active lock for the same currency pair
        var existingLockForPair = activeLocks.FirstOrDefault(rl =>
            rl.IsValid() &&
            !rl.IsUsed &&
            rl.BaseCurrency == baseCurrency &&
            rl.TargetCurrency == targetCurrency);

        if (existingLockForPair != null)
        {
            return RateLockAvailability.Unavailable(
                $"You already have an active rate lock for {baseCurrency.Code}/{targetCurrency.Code} (valid until {existingLockForPair.ValidUntil:g})");
        }

        return RateLockAvailability.Available(
            currentCount,
            rateLockingSettings.Value.MaxActiveLocksPerClient,
            rateLockingSettings.Value.MaxLockDuration);
    }

    private RateLockResponse BuildRateLockResponse(RateLock rateLock)
    {
        var now = DateTime.UtcNow;
        var timeRemaining = rateLock.ValidUntil - now;

        return new RateLockResponse
        {
            RateLockId = rateLock.Id,
            BaseCurrency = rateLock.BaseCurrency.Code,
            TargetCurrency = rateLock.TargetCurrency.Code,
            LockedRate = rateLock.LockedRate,
            LockedAt = rateLock.LockedAt,
            ValidUntil = rateLock.ValidUntil,
            IsUsed = rateLock.IsUsed,
            IsValid = rateLock.IsValid(),
            IsExpiringSoon = rateLock.IsExpiringSoon(rateLockingSettings.Value.ExpirationWarningThreshold),
            ExpirationWarning = rateLock.GetExpirationWarning(),
            Reference = rateLock.LockReference,
            TimeRemaining = timeRemaining,
            UsedAt = rateLock.UsedAt
        };
    }
}

// Helper record for rate lock availability
public record RateLockAvailability
{
    public bool CanCreateNewLock { get; init; }
    public string? Reason { get; init; }
    public int CurrentActiveLocks { get; init; }
    public int MaxAllowedLocks { get; init; }
    public TimeSpan MaxLockDuration { get; init; }

    public static RateLockAvailability Available(int currentLocks, int maxLocks, TimeSpan maxDuration)
    {
        return new RateLockAvailability
        {
            CanCreateNewLock = true,
            CurrentActiveLocks = currentLocks,
            MaxAllowedLocks = maxLocks,
            MaxLockDuration = maxDuration
        };
    }

    public static RateLockAvailability Unavailable(string reason)
    {
        return new RateLockAvailability
        {
            CanCreateNewLock = false,
            Reason = reason
        };
    }
}

