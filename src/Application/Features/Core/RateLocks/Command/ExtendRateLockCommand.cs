using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TegWallet.Application.Features.Core.RateLocks.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.RateLocks.Command;

// Extend a rate lock
public record ExtendRateLockCommand : IRequest<Result<RateLockResponse>>
{
    public Guid ClientId { get; init; }
    public Guid RateLockId { get; init; }
    public TimeSpan? AdditionalDuration { get; init; }
}

public class ExtendRateLockCommandHandler(
    UserManager<Client> userManager,
    IRateLockRepository rateLockRepository,
    IOptions<RateLockingSettings> rateLockingSettings,
    IAppLocalizer localizer)
    : IRequestHandler<ExtendRateLockCommand, Result<RateLockResponse>>
{
    public async Task<Result<RateLockResponse>> Handle(ExtendRateLockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if rate lock extension is allowed
            if (!rateLockingSettings.Value.AllowLockExtension)
            {
                return Result<RateLockResponse>.Failed(localizer["Rate lock extension is not allowed"]);
            }

            // Validate additional duration
            var additionalDuration = request.AdditionalDuration ?? rateLockingSettings.Value.ExtensionDuration;

            if (additionalDuration <= TimeSpan.Zero)
                return Result<RateLockResponse>.Failed(localizer["Additional duration must be positive"]);

            // Check if extension duration exceeds maximum allowed
            if (additionalDuration > rateLockingSettings.Value.ExtensionDuration)
            {
                return Result<RateLockResponse>.Failed(
                    localizer[$"Extension duration exceeds maximum allowed duration of {rateLockingSettings.Value.ExtensionDuration:hh\\:mm} hours"]);
            }

            // Get client to verify existence
            var client = await userManager.FindByIdAsync(request.ClientId.ToString());
            if (client == null)
                return Result<RateLockResponse>.Failed(localizer["Client not found"]);

            // Use repository to extend the rate lock
            var extensionResult = await rateLockRepository.ExtendRateLockAsync(
                request.RateLockId,
                request.ClientId,
                additionalDuration);

            if (extensionResult.Status != RepositoryActionStatus.Updated)
            {
                var errorMessage = extensionResult.Status switch
                {
                    RepositoryActionStatus.NotFound => localizer["Rate lock not found"],
                    RepositoryActionStatus.Invalid => extensionResult.Exception?.Message ?? localizer["Cannot extend rate lock"],
                    RepositoryActionStatus.ConcurrencyConflict => localizer["A concurrency conflict occurred. Please try again."],
                    RepositoryActionStatus.Deadlock => localizer["A system deadlock occurred. Please try again."],
                    RepositoryActionStatus.Error => localizer["An error occurred while extending the rate lock"],
                    _ => localizer["An unexpected error occurred while extending the rate lock"]
                };

                return Result<RateLockResponse>.Failed(errorMessage);
            }

            // Build response from the extended rate lock
            var extendedRateLock = extensionResult.Entity!;
            var response = BuildRateLockResponse(extendedRateLock);

            return Result<RateLockResponse>.Succeeded(response);
        }
        catch (DomainException ex)
        {
            return Result<RateLockResponse>.Failed(ex.Message);
        }
        catch (Exception)
        {
            return Result<RateLockResponse>.Failed(localizer["An unexpected error occurred while extending the rate lock"]);
        }
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