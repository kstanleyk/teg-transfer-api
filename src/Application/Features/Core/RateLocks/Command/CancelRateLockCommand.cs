using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.RateLocks.Command;

// Cancel a rate lock

public record CancelRateLockCommand : IRequest<Result>
{
    public Guid ClientId { get; init; }
    public Guid RateLockId { get; init; }
    public string Reason { get; init; } = "Cancelled by user";
}

public class CancelRateLockCommandHandler(
    UserManager<Client> userManager,
    IRateLockRepository rateLockRepository,
    IAppLocalizer localizer) : IRequestHandler<CancelRateLockCommand, Result>
{
    public async Task<Result> Handle(CancelRateLockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result.Failed(localizer["Cancellation reason is required"]);

            // Get client to verify existence
            var client = await userManager.FindByIdAsync(request.ClientId.ToString());
            if (client == null)
                return Result.Failed(localizer["Client not found"]);

            // Use repository to cancel the rate lock
            var cancellationResult = await rateLockRepository.CancelRateLockAsync(
                request.RateLockId, request.ClientId, request.Reason);

            if (cancellationResult.Status != RepositoryActionStatus.Updated &&
                cancellationResult.Status != RepositoryActionStatus.Deleted)
            {
                var errorMessage = cancellationResult.Status switch
                {
                    RepositoryActionStatus.NotFound => localizer["Rate lock not found"],
                    RepositoryActionStatus.Invalid => cancellationResult.Exception?.Message ?? localizer["Cannot cancel rate lock"],
                    RepositoryActionStatus.ConcurrencyConflict => localizer["A concurrency conflict occurred. Please try again."],
                    RepositoryActionStatus.Deadlock => localizer["A system deadlock occurred. Please try again."],
                    RepositoryActionStatus.Error => localizer["An error occurred while cancelling the rate lock"],
                    _ => localizer["An unexpected error occurred while cancelling the rate lock"]
                };

                return Result.Failed(errorMessage);
            }

            return Result.Succeeded(localizer["Rate lock cancelled successfully"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception)
        {
            return Result.Failed(localizer["An unexpected error occurred while cancelling the rate lock"]);
        }
    }
}