using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Command;

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
    IClientRepository clientRepository,
    IAppLocalizer localizer) :RequestHandlerBase, IRequestHandler<CreateIndividualExchangeRateCommand, Result<Guid>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
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
            var client = await _clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result<Guid>.Failed("Client not found");

            if (client.Status != ClientStatus.Active)
                return Result<Guid>.Failed("Cannot create rate for inactive client");

            var marginPercentage = command.Margin / 100;

            var parameters = new CreateIndividualExchangeRateParameters(
                command.BaseCurrency,
                command.TargetCurrency,
                command.BaseCurrencyValue,
                command.TargetCurrencyValue,
                marginPercentage,
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

    protected override void DisposeCore()
    {
        _exchangeRateRepository.Dispose();
        _clientRepository.Dispose();
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