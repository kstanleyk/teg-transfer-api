using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.RateLocks.Dtos;

public record RateLockAvailabilityResponse
{
    public bool IsAvailable { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
    public TimeSpan MaxLockDuration { get; init; }
    public int CurrentActiveLocks { get; init; }
    public int MaxAllowedLocks { get; init; }
    public bool CanCreateNewLock { get; init; }
    public string? Reason { get; init; }
}