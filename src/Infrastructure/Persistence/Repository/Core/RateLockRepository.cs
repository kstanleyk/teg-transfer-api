using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class RateLockRepository( IExchangeRateRepository exchangeRateRepository, IDatabaseFactory databaseFactory)
    : DataRepository<RateLock, Guid>(databaseFactory), IRateLockRepository
{
    public async Task<RateLock?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(rl => rl.ExchangeRate)
            .Include(rl => rl.Client)
            .FirstOrDefaultAsync(rl => rl.Id == id);
    }

    public async Task<RateLock?> GetValidRateLockAsync(Guid rateLockId, Guid clientId)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Include(rl => rl.ExchangeRate)
            .Include(rl => rl.Client)
            .Where(rl => rl.Id == rateLockId &&
                        rl.ClientId == clientId &&
                        !rl.IsUsed &&
                        rl.ValidUntil > now)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RateLock>> GetClientRateLocksAsync(Guid clientId, bool includeExpired = false)
    {
        var query = DbSet
            .Include(rl => rl.ExchangeRate)
            .Include(rl => rl.Client)
            .Where(rl => rl.ClientId == clientId);

        if (!includeExpired)
        {
            query = query.Where(rl => !rl.IsUsed && rl.ValidUntil > DateTime.UtcNow);
        }

        return await query
            .OrderByDescending(rl => rl.LockedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<RateLock>> GetExpiringRateLocksAsync(TimeSpan threshold)
    {
        var expiryThreshold = DateTime.UtcNow.Add(threshold);

        return await DbSet
            .Include(rl => rl.Client)
            .Include(rl => rl.ExchangeRate)
            .Where(rl => !rl.IsUsed &&
                        rl.ValidUntil <= expiryThreshold &&
                        rl.ValidUntil > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<bool> HasActiveRateLockAsync(Guid clientId, Currency baseCurrency, Currency targetCurrency)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .AnyAsync(rl => rl.ClientId == clientId &&
                           rl.BaseCurrency.Code == baseCurrency.Code &&
                           rl.TargetCurrency.Code == targetCurrency.Code &&
                           !rl.IsUsed &&
                           rl.ValidUntil > now);
    }

    public async Task<int> CleanupExpiredRateLocksAsync()
    {
        var expiredDate = DateTime.UtcNow.AddDays(-7); // Clean up expired locks older than 7 days

        var expiredLocks = await DbSet
            .Where(rl => rl.ValidUntil < expiredDate || rl.IsUsed)
            .ToListAsync();

        DbSet.RemoveRange(expiredLocks);
        return await Context.SaveChangesAsync();
    }

    public async Task<RepositoryActionResult<RateLock>> ExtendRateLockAsync(Guid rateLockId, Guid clientId,
        TimeSpan additionalDuration)
    {
        try
        {
            // Get rate lock with client validation and include exchange rate
            var rateLock = await DbSet
                .Include(rl => rl.ExchangeRate)
                .Include(rl => rl.Client)
                .FirstOrDefaultAsync(rl => rl.Id == rateLockId && rl.ClientId == clientId);

            if (rateLock == null)
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.NotFound);

            // Validate rate lock state
            if (rateLock.IsUsed)
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot extend a used rate lock"));

            if (!rateLock.IsValid())
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot extend an expired rate lock"));

            // Check exchange rate validity for extension
            var exchangeRateCheck = await CheckExchangeRateValidityAsync(rateLock, additionalDuration);
            if (!exchangeRateCheck.Entity)
                return new RepositoryActionResult<RateLock>(null, exchangeRateCheck.Status, exchangeRateCheck.Exception);

            // Store original expiration for audit
            //var originalValidUntil = rateLock.ValidUntil;

            // Extend the rate lock
            rateLock.Extend(additionalDuration);

            // Update in database
            DbSet.Update(rateLock);
            await Context.SaveChangesAsync();

            //_logger.LogInformation(
            //    "Rate lock {RateLockId} extended by {AdditionalDuration}. Original: {OriginalValidUntil}, New: {NewValidUntil}",
            //    rateLockId, additionalDuration, originalValidUntil, rateLock.ValidUntil);

            return new RepositoryActionResult<RateLock>(rateLock, RepositoryActionStatus.Updated);
        }
        catch (DomainException ex)
        {
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error extending rate lock {RateLockId} for client {ClientId}", rateLockId, clientId);
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<RateLock>> CancelRateLockAsync(Guid rateLockId, Guid clientId, string reason)
    {
        try
        {
            // Get rate lock with client validation
            var rateLock = await DbSet
                .Include(rl => rl.ExchangeRate)
                .Include(rl => rl.Client)
                .FirstOrDefaultAsync(rl => rl.Id == rateLockId && rl.ClientId == clientId);

            if (rateLock == null)
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.NotFound);

            // Validate rate lock state
            if (rateLock.IsUsed)
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot cancel a used rate lock"));

            if (!rateLock.IsValid())
                return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot cancel an expired rate lock"));

            // Cancel the rate lock by marking it as used with the cancellation reason
            rateLock.MarkAsCancelled(reason);

            // Update in database
            DbSet.Update(rateLock);
            await Context.SaveChangesAsync();

            return new RepositoryActionResult<RateLock>(rateLock, RepositoryActionStatus.Updated);
        }
        catch (DomainException ex)
        {
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Invalid, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<RateLock>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<bool>> CanExtendRateLockAsync(Guid rateLockId, TimeSpan additionalDuration)
    {
        try
        {
            // Get rate lock with exchange rate
            var rateLock = await DbSet
                .Include(rl => rl.ExchangeRate)
                .FirstOrDefaultAsync(rl => rl.Id == rateLockId);

            if (rateLock == null)
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.NotFound);

            // Validate rate lock state
            if (rateLock.IsUsed)
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot extend a used rate lock"));

            if (!rateLock.IsValid())
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Invalid,
                    new DomainException("Cannot extend an expired rate lock"));

            // Check exchange rate validity
            var exchangeRateCheck = await CheckExchangeRateValidityAsync(rateLock, additionalDuration);
            if (!exchangeRateCheck.Entity)
                return new RepositoryActionResult<bool>(false, exchangeRateCheck.Status, exchangeRateCheck.Exception);

            // Check total duration limits
            var totalDuration = (rateLock.ValidUntil - rateLock.LockedAt) + additionalDuration;
            var maximumTotalDuration = TimeSpan.FromDays(30);

            if (totalDuration > maximumTotalDuration)
            {
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Invalid,
                    new DomainException($"Total rate lock duration cannot exceed {maximumTotalDuration.TotalDays} days"));
            }

            return new RepositoryActionResult<bool>(true, RepositoryActionStatus.Okay);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error checking if rate lock {RateLockId} can be extended", rateLockId);
            return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Error, ex);
        }
    }

    private async Task<RepositoryActionResult<bool>> CheckExchangeRateValidityAsync(RateLock rateLock,
        TimeSpan additionalDuration)
    {
        try
        {
            // Get current state of the exchange rate
            var exchangeRateResult = await exchangeRateRepository.GetByIdAsync(rateLock.ExchangeRateId);
            if (exchangeRateResult == null)
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.NotFound,
                    new DomainException("Original exchange rate no longer exists"));

            var exchangeRate = exchangeRateResult;

            if (!exchangeRate.IsActive)
                return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Invalid,
                    new DomainException("The original exchange rate is no longer active"));

            // Check if we're not extending beyond the exchange rate's effective period
            if (exchangeRate.EffectiveTo.HasValue)
            {
                var newExpiration = rateLock.ValidUntil.Add(additionalDuration);
                if (newExpiration > exchangeRate.EffectiveTo.Value)
                {
                    return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Invalid,
                        new DomainException("Cannot extend rate lock beyond the original exchange rate's validity period"));
                }
            }

            return new RepositoryActionResult<bool>(true, RepositoryActionStatus.Okay);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<bool>(false, RepositoryActionStatus.Error, ex);
        }
    }
}