using MediatR;
using TegWallet.Application.Features.Core.RateLocks.Dtos;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.RateLocks.Query;

// Get rate lock details
public record GetRateLockDetailsQuery : IRequest<Result<RateLockResponse>>
{
    public Guid ClientId { get; init; }
    public Guid RateLockId { get; init; }
}