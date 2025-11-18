using MediatR;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

public record UpdateMinimumAmountConfigurationCommand(
    Guid ConfigurationId,
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null,
    string UpdatedBy = "SYSTEM") : IRequest<Result>;

public class UpdateMinimumAmountConfigurationCommandHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository,
    IAppLocalizer localizer)
    : IRequestHandler<UpdateMinimumAmountConfigurationCommand, Result>
{
    private readonly IMinimumAmountConfigurationRepository _minimumAmountConfigurationRepository = minimumAmountConfigurationRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(UpdateMinimumAmountConfigurationCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateMinimumAmountConfigurationCommandValidator();
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
            var parameters = new UpdateMinimumAmountConfigurationParameters(
                command.ConfigurationId,
                command.MinimumAmount,
                command.EffectiveFrom,
                command.UpdatedBy,
                command.EffectiveTo);

            var result = await _minimumAmountConfigurationRepository.UpdateAsync(parameters);

            return result.Status switch
            {
                RepositoryActionStatus.Updated => Result.Succeeded(_localizer["MinimumAmountConfigurationUpdatedSuccess"]),
                RepositoryActionStatus.NotFound => Result.Failed("Minimum amount configuration not found"),
                RepositoryActionStatus.NothingModified => Result.Failed("No changes were made to the minimum amount configuration"),
                _ => Result.Failed("An unexpected error occurred while updating the minimum amount configuration")
            };
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to update minimum amount configuration: {ex.Message}");
        }
    }
}

public record UpdateMinimumAmountConfigurationParameters(
    Guid ConfigurationId,
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    string UpdatedBy,
    DateTime? EffectiveTo = null);