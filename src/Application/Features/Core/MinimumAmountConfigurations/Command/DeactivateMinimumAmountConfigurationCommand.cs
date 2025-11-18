using MediatR;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

public record DeactivateMinimumAmountConfigurationCommand(
    Guid ConfigurationId,
    string Reason,
    string DeactivatedBy = "SYSTEM") : IRequest<Result>;

public class DeactivateMinimumAmountConfigurationCommandHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository,
    IAppLocalizer localizer)
    : IRequestHandler<DeactivateMinimumAmountConfigurationCommand, Result>
{
    private readonly IMinimumAmountConfigurationRepository _minimumAmountConfigurationRepository = minimumAmountConfigurationRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(DeactivateMinimumAmountConfigurationCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeactivateMinimumAmountConfigurationCommandValidator();
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
            var parameters = new DeactivateMinimumAmountConfigurationParameters(
                command.ConfigurationId,
                command.Reason,
                command.DeactivatedBy);

            var result = await _minimumAmountConfigurationRepository.DeactivateAsync(parameters);

            return result.Status switch
            {
                RepositoryActionStatus.Updated => Result.Succeeded(_localizer["MinimumAmountConfigurationDeactivatedSuccess"]),
                RepositoryActionStatus.NotFound => Result.Failed("Minimum amount configuration not found"),
                RepositoryActionStatus.NothingModified => Result.Failed("Minimum amount configuration was already deactivated"),
                _ => Result.Failed("An unexpected error occurred while deactivating the minimum amount configuration")
            };
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to deactivate minimum amount configuration: {ex.Message}");
        }
    }
}

public record DeactivateMinimumAmountConfigurationParameters(
    Guid ConfigurationId,
    string Reason,
    string DeactivatedBy);