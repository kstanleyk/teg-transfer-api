using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ExchangeRates.Command;

// Update Exchange Rate (any type)
public record UpdateExchangeRateCommand(
    Guid ExchangeRateId,
    decimal NewBaseCurrencyValue,
    decimal NewTargetCurrencyValue,
    decimal NewMargin,
    string UpdatedBy,
    string Reason = "Rate updated") : IRequest<Result>;

public class UpdateExchangeRateCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IAppLocalizer localizer) : IRequestHandler<UpdateExchangeRateCommand, Result>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(UpdateExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateExchangeRateCommandValidator();
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
            var parameters = new UpdateExchangeRateParameters(
                command.ExchangeRateId,
                command.NewBaseCurrencyValue,
                command.NewTargetCurrencyValue,
                command.NewMargin,
                command.UpdatedBy,
                command.Reason);

            var result = await _exchangeRateRepository.UpdateExchangeRateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Updated)
                return Result.Failed("An unexpected error occurred while updating the exchange rate. Please try again.");

            var message = _localizer["ExchangeRateUpdatedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to update exchange rate: {ex.Message}");
        }
    }
}

public record UpdateExchangeRateParameters(
    Guid ExchangeRateId,
    decimal NewBaseCurrencyValue,
    decimal NewTargetCurrencyValue,
    decimal NewMargin,
    string UpdatedBy,
    string Reason = "Rate updated");