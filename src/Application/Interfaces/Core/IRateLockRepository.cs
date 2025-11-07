using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Interfaces.Core;

public interface IRateLockRepository : IRepository<RateLock, Guid>
{
    Task<RateLock?> GetByIdAsync(Guid id);
    Task<RateLock?> GetValidRateLockAsync(Guid rateLockId, Guid clientId);
    Task<IEnumerable<RateLock>> GetClientRateLocksAsync(Guid clientId, bool includeExpired = false);
    Task<IEnumerable<RateLock>> GetExpiringRateLocksAsync(TimeSpan threshold);
    Task<bool> HasActiveRateLockAsync(Guid clientId, Currency baseCurrency, Currency targetCurrency);
    Task<int> CleanupExpiredRateLocksAsync();
    Task<RepositoryActionResult<RateLock>> ExtendRateLockAsync(Guid rateLockId, Guid clientId, TimeSpan additionalDuration);
    Task<RepositoryActionResult<bool>> CanExtendRateLockAsync(Guid rateLockId, TimeSpan additionalDuration);
    Task<RepositoryActionResult<RateLock>> CancelRateLockAsync(Guid rateLockId, Guid clientId, string reason);
}