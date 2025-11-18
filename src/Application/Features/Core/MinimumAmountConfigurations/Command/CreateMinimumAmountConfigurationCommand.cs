using MediatR;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

public record CreateMinimumAmountConfigurationCommand(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null,
    string CreatedBy = "SYSTEM") : IRequest<Result<Guid>>;

public class CreateMinimumAmountConfigurationCommandHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository,
    IAppLocalizer localizer)
    : IRequestHandler<CreateMinimumAmountConfigurationCommand, Result<Guid>>
{
    private readonly IMinimumAmountConfigurationRepository _minimumAmountConfigurationRepository = minimumAmountConfigurationRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateMinimumAmountConfigurationCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateMinimumAmountConfigurationCommandValidator();
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
            // Parse currencies
            if (!Currency.TryFromCode(command.BaseCurrencyCode, out var baseCurrency))
                return Result<Guid>.Failed($"Invalid base currency: {command.BaseCurrencyCode}");

            if (!Currency.TryFromCode(command.TargetCurrencyCode, out var targetCurrency))
                return Result<Guid>.Failed($"Invalid target currency: {command.TargetCurrencyCode}");

            // Check for overlapping configurations
            var overlappingConfigs = await _minimumAmountConfigurationRepository.GetOverlappingConfigurationsAsync(
                baseCurrency, targetCurrency, command.EffectiveFrom, command.EffectiveTo);

            if (overlappingConfigs.Any())
                return Result<Guid>.Failed("An active minimum amount configuration already exists for this currency pair during the specified period");

            var parameters = new CreateMinimumAmountConfigurationParameters(
                baseCurrency,
                targetCurrency,
                command.MinimumAmount,
                command.EffectiveFrom,
                command.CreatedBy,
                command.EffectiveTo);

            var result = await _minimumAmountConfigurationRepository.CreateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
                return Result<Guid>.Failed("An unexpected error occurred while creating the minimum amount configuration. Please try again.");

            var message = _localizer["MinimumAmountConfigurationCreatedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, message);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to create minimum amount configuration: {ex.Message}");
        }
    }
}

public record CreateMinimumAmountConfigurationParameters(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    string CreatedBy,
    DateTime? EffectiveTo = null);