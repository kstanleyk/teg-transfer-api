using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Command;

// Create General Exchange Rate (applies to all clients)
public record CreateGeneralExchangeRateCommand(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null,
    List<ExchangeRateTierRequest>? Tiers = null) : IRequest<Result<Guid>>; // NEW: Tiers parameter

public record ExchangeRateTierRequest(
    decimal MinAmount,
    decimal MaxAmount,
    decimal Rate,
    decimal Margin,  // This is in percentage (e.g., 3.0 for 3%)
    string CreatedBy = "SYSTEM");

public record CreateGeneralExchangeRateParameters(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null,
    List<ExchangeRateTierRequest>? Tiers = null); // NEW: Tiers parameter


public class CreateGeneralExchangeRateCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IAppLocalizer localizer)
    : IRequestHandler<CreateGeneralExchangeRateCommand, Result<Guid>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateGeneralExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateGeneralExchangeRateCommandValidator();
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
            var marginPercentage = command.Margin / 100;

            // Convert tier DTOs to domain objects if provided
            List<ExchangeRateTierRequest>? tierParameters = null;
            if (command.Tiers != null && command.Tiers.Any())
            {
                tierParameters = command.Tiers.Select(t => new ExchangeRateTierRequest(
                    t.MinAmount,
                    t.MaxAmount,
                    t.Rate,
                    t.Margin / 100, // Convert margin percentage to decimal
                    t.CreatedBy
                )).ToList();
            }

            var parameters = new CreateGeneralExchangeRateParameters(
                command.BaseCurrency,
                command.TargetCurrency,
                command.BaseCurrencyValue,
                command.TargetCurrencyValue,
                marginPercentage,
                command.EffectiveFrom,
                command.CreatedBy,
                command.Source,
                command.EffectiveTo,
                tierParameters); // Pass tiers to repository

            var result = await _exchangeRateRepository.CreateGeneralExchangeRateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
                return Result<Guid>.Failed("An unexpected error occurred while creating the exchange rate. Please try again.");

            var message = _localizer["ExchangeRateCreatedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, message);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to create general exchange rate: {ex.Message}");
        }
    }
}