using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Extend Exchange Rate Validity
public record ExtendExchangeRateValidityCommand(
    Guid ExchangeRateId,
    DateTime NewEffectiveTo,
    string UpdatedBy,
    string Reason = "Validity extended") : IRequest<Result>;

public class ExtendExchangeRateValidityCommandHandler(
    IExchangeRateRepository exchangeRateRepository,
    IAppLocalizer localizer) : IRequestHandler<ExtendExchangeRateValidityCommand, Result>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(ExtendExchangeRateValidityCommand command, CancellationToken cancellationToken)
    {
        var validator = new ExtendExchangeRateValidityCommandValidator();
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
            var parameters = new ExtendExchangeRateValidityParameters(
                command.ExchangeRateId,
                command.NewEffectiveTo,
                command.UpdatedBy,
                command.Reason);

            var result = await _exchangeRateRepository.ExtendExchangeRateValidityAsync(parameters);

            if (result.Status != RepositoryActionStatus.Updated)
                return Result.Failed("An unexpected error occurred while extending the exchange rate validity. Please try again.");

            var message = _localizer["ExchangeRateValidityExtendedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to extend exchange rate validity: {ex.Message}");
        }
    }
}

public record ExtendExchangeRateValidityParameters(
    Guid ExchangeRateId,
    DateTime NewEffectiveTo,
    string UpdatedBy,
    string Reason = "Validity extended");