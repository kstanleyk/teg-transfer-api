using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Command;

// Create Group Exchange Rate (applies to a client group)
public record CreateGroupExchangeRateCommand(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientGroupId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null) : IRequest<Result<Guid>>;

public class CreateGroupExchangeRateCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer) : IRequestHandler<CreateGroupExchangeRateCommand, Result<Guid>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateGroupExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateGroupExchangeRateCommandValidator();
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
            // Validate client group exists and is active
            var clientGroup = await _clientGroupRepository.GetAsync(command.ClientGroupId);
            if (clientGroup == null)
                return Result<Guid>.Failed("Client group not found");

            if (!clientGroup.IsActive)
                return Result<Guid>.Failed("Cannot create rate for inactive client group");

            var marginPercentage = command.Margin / 100;

            var parameters = new CreateGroupExchangeRateParameters(
                command.BaseCurrency,
                command.TargetCurrency,
                command.BaseCurrencyValue,
                command.TargetCurrencyValue,
                marginPercentage,
                command.ClientGroupId,
                command.EffectiveFrom,
                command.CreatedBy,
                command.Source,
                command.EffectiveTo);

            var result = await _exchangeRateRepository.CreateGroupExchangeRateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
                return Result<Guid>.Failed("An unexpected error occurred while creating the group exchange rate. Please try again.");

            var message = _localizer["GroupExchangeRateCreatedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, message);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to create group exchange rate: {ex.Message}");
        }
    }
}

public record CreateGroupExchangeRateParameters(
    Currency BaseCurrency,
    Currency TargetCurrency,
    decimal BaseCurrencyValue,
    decimal TargetCurrencyValue,
    decimal Margin,
    Guid ClientGroupId,
    DateTime EffectiveFrom,
    string CreatedBy = "SYSTEM",
    string Source = "Market",
    DateTime? EffectiveTo = null);