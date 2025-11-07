using MediatR;
using TegWallet.Application.Features.Core.RateLocks.Dtos;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.RateLocks.Query;

// Get client's active rate locks
public record GetClientRateLocksQuery : IRequest<Result<IEnumerable<RateLockResponse>>>
{
    public Guid ClientId { get; init; }
    public bool IncludeExpired { get; init; } = false;
    public bool IncludeUsed { get; init; } = false;
}