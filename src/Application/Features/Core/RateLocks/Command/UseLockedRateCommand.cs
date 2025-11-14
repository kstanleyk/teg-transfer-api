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

// Use a locked rate for purchase calculation
public record UseLockedRateCommand : IRequest<Result<CalculatePurchaseAmountResponse>>
{
    public Guid ClientId { get; init; }
    public Guid RateLockId { get; init; }
    public decimal TargetAmount { get; init; }
    public decimal ServiceFeePercentage { get; init; } = 0.02m;
}

public class UseLockedRateCommandHandler(
    IClientRepository clientRepository,
    IRateLockRepository rateLockRepository,
    IAppLocalizer localizer,
    IOptions<RateLockingSettings> rateLockingSettings)
    : IRequestHandler<UseLockedRateCommand, Result<CalculatePurchaseAmountResponse>>
{
    public async Task<Result<CalculatePurchaseAmountResponse>> Handle(
        UseLockedRateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (request.TargetAmount <= 0)
                return Result<CalculatePurchaseAmountResponse>.Failed(localizer["Target amount must be positive"]);

            if (request.ServiceFeePercentage < 0 || request.ServiceFeePercentage > 1)
                return Result<CalculatePurchaseAmountResponse>.Failed(localizer["Service fee percentage must be between 0 and 1"]);

            // Get and validate rate lock
            var rateLock = await rateLockRepository.GetValidRateLockAsync(request.RateLockId, request.ClientId);
            if (rateLock == null)
                return Result<CalculatePurchaseAmountResponse>.Failed(localizer["Rate lock not found or no longer valid"]);

            // Get client
            var client = await clientRepository.GetAsync(request.ClientId);
            if (client == null)
                return Result<CalculatePurchaseAmountResponse>.Failed(localizer["Client not found"]);

            // Validate currency match
            if (client.Wallet.BaseCurrency != rateLock.BaseCurrency)
            {
                return Result<CalculatePurchaseAmountResponse>.Failed(
                    localizer[$"Rate lock currency ({rateLock.BaseCurrency.Code}) does not match client's base currency ({client.Wallet.BaseCurrency.Code})"]);
            }

            // Perform calculation
            var (requiredBaseAmount, serviceFeeAmount, totalBaseAmount, effectiveRate) =
                PerformCalculation(request.TargetAmount, rateLock.LockedRate, request.ServiceFeePercentage, rateLock.TargetCurrency);

            // Create response using helper method
            var response = CalculatePurchaseAmountResponse.CreateFromRateLock(
                rateLock,
                client.Wallet.BaseCurrency,
                request.TargetAmount,
                request.ServiceFeePercentage,
                requiredBaseAmount,
                serviceFeeAmount,
                totalBaseAmount);

            // Set expiration warnings based on current settings
            response = response with
            {
                IsRateExpiringSoon = rateLock.IsExpiringSoon(rateLockingSettings.Value.ExpirationWarningThreshold),
                RateExpirationWarning = rateLock.GetExpirationWarning()
            };

            return Result<CalculatePurchaseAmountResponse>.Succeeded(response);
        }
        catch (DomainException ex)
        {
            return Result<CalculatePurchaseAmountResponse>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<CalculatePurchaseAmountResponse>.Failed(localizer["An unexpected error occurred while using the rate lock"]);
        }
    }

    private (decimal requiredBaseAmount, decimal serviceFeeAmount, decimal totalBaseAmount, decimal effectiveRate)
        PerformCalculation(decimal targetAmount, decimal lockedRate, decimal serviceFeePercentage, Currency targetCurrency)
    {
        if (lockedRate <= 0)
            throw new DomainException("Locked rate must be positive");

        decimal requiredBaseAmount = targetAmount / lockedRate;
        decimal serviceFeeAmount = requiredBaseAmount * serviceFeePercentage;
        decimal totalBaseAmount = requiredBaseAmount + serviceFeeAmount;

        return (
            Math.Round(requiredBaseAmount, 2),
            Math.Round(serviceFeeAmount, 2),
            Math.Round(totalBaseAmount, 2),
            lockedRate
        );
    }
}
