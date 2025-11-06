using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Deactivate Exchange Rate
public record DeactivateExchangeRateCommand(
    Guid ExchangeRateId,
    string DeactivatedBy,
    string Reason = "Rate deactivated") : IRequest<Result>;

public class DeactivateExchangeRateCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IAppLocalizer localizer) : IRequestHandler<DeactivateExchangeRateCommand, Result>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(DeactivateExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeactivateExchangeRateCommandValidator();
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
            var parameters = new DeactivateExchangeRateParameters(
                command.ExchangeRateId,
                command.DeactivatedBy,
                command.Reason);

            var result = await _exchangeRateRepository.DeactivateExchangeRateAsync(parameters);

            if (result.Status != RepositoryActionStatus.Updated)
                return Result.Failed("An unexpected error occurred while deactivating the exchange rate. Please try again.");

            var message = _localizer["ExchangeRateDeactivatedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to deactivate exchange rate: {ex.Message}");
        }
    }
}

public record DeactivateExchangeRateParameters(
    Guid ExchangeRateId,
    string DeactivatedBy,
    string Reason = "Rate deactivated");