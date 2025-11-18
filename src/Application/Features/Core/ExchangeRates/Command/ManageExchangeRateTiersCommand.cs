using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Features.Core.ExchangeRates.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates.Command;

public record ManageExchangeRateTiersCommand(
    Guid ExchangeRateId,
    List<ExchangeRateTierRequestDto> Tiers) : IRequest<Result>;

public class ManageExchangeRateTiersCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository,
    IAppLocalizer localizer)
    : IRequestHandler<ManageExchangeRateTiersCommand, Result>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IMinimumAmountConfigurationRepository _minimumAmountConfigurationRepository = minimumAmountConfigurationRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(ManageExchangeRateTiersCommand command, CancellationToken cancellationToken)
    {
        // Step 1: Basic validation (no database calls)
        var validator = new ManageExchangeRateTiersCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        try
        {
            // Step 2: Business logic validation (with database calls)
            var businessValidationResult = await ValidateBusinessRules(command);
            if (!businessValidationResult.Success)
            {
                return businessValidationResult;
            }

            // Step 3: Convert and process tiers
            var tierRequests = command.Tiers.Select(t => new ExchangeRateTierRequest(
                t.MinAmount,
                t.MaxAmount,
                t.Rate,
                t.Margin / 100, // Convert percentage to decimal
                t.CreatedBy
            )).ToList();

            await _exchangeRateRepository.ManageExchangeRateTiersAsync(command.ExchangeRateId, tierRequests);

            return Result.Succeeded(_localizer["ExchangeRateTiersManagedSuccess"]);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return Result.Failed("Exchange rate not found");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("overlap"))
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to manage exchange rate tiers: {ex.Message}");
        }
    }

    private async Task<Result> ValidateBusinessRules(ManageExchangeRateTiersCommand command)
    {
        // 1. Validate exchange rate exists and is general type
        var exchangeRate = await _exchangeRateRepository.GetByIdAsync(command.ExchangeRateId);
        if (exchangeRate == null)
        {
            return Result.Failed("Exchange rate not found");
        }

        if (exchangeRate.Type != RateType.General)
        {
            return Result.Failed("Tiers can only be added to general exchange rates");
        }

        // 2. Validate last tier max equals minimum amount configuration
        var minAmountConfig = await _minimumAmountConfigurationRepository.GetApplicableMinimumAmountAsync(
            exchangeRate.BaseCurrency,
            exchangeRate.TargetCurrency,
            DateTime.UtcNow);

        if (minAmountConfig == null)
        {
            return Result.Failed("No minimum amount configuration found for this currency pair. Please create a minimum amount configuration first.");
        }

        var lastTier = command.Tiers.OrderBy(t => t.MaxAmount).Last();
        if (lastTier.MaxAmount != minAmountConfig.MinimumAmount)
        {
            return Result.Failed($"The last tier's maximum amount ({lastTier.MaxAmount}) must equal the minimum amount configuration ({minAmountConfig.MinimumAmount}) for {exchangeRate.BaseCurrency.Code}/{exchangeRate.TargetCurrency.Code}");
        }

        return Result.Succeeded();
    }
}