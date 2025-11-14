using MediatR;
using TegWallet.Application.Features.Core.RateLocks.Dtos;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.RateLocks.Query;

// Check if rate locking is available for a currency pair
public record CheckRateLockAvailabilityQuery : IRequest<Result<RateLockAvailabilityResponse>>
{
    public Guid ClientId { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
}
