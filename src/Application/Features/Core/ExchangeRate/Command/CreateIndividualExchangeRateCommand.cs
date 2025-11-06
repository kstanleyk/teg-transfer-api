using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.ExchangeRate.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Create Individual Exchange Rate (applies to a specific client)
public record CreateIndividualExchangeRateCommand(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Manual",
    DateTime? EffectiveTo = null) : IRequest<Result<Guid>>;

public class CreateIndividualExchangeRateCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    UserManager<Domain.Entity.Core.Client> userManager,
    IAppLocalizer localizer) : IRequestHandler<CreateIndividualExchangeRateCommand, Result<Guid>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly UserManager<Domain.Entity.Core.Client> _userManager = userManager;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateIndividualExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateIndividualExchangeRateCommandValidator();
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
            // Validate client exists and is active
            var client = await _userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result<Guid>.Failed("Client not found");

            if (client.Status != ClientStatus.Active)
                return Result<Guid>.Failed("Cannot create rate for inactive client");

            var parameters = new CreateIndividualExchangeRateParameters(
                command.BaseCurrency,
                command.TargetCurrency,
                command.BaseCurrencyValue,
                command.TargetCurrencyValue,
                command.Margin,
                command.ClientId,
                command.EffectiveFrom,
                command.CreatedBy,
                command.Source,
                command.EffectiveTo);

            var result = await _exchangeRateRepository.CreateIndividualExchangeRateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
                return Result<Guid>.Failed("An unexpected error occurred while creating the individual exchange rate. Please try again.");

            var message = _localizer["IndividualExchangeRateCreatedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, message);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to create individual exchange rate: {ex.Message}");
        }
    }
}

public record CreateIndividualExchangeRateParameters(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Manual",
    DateTime? EffectiveTo = null);