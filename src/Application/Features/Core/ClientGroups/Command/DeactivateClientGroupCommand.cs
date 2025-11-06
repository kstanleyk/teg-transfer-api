using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ClientGroups.Command;

public record DeactivateClientGroupCommand(
    Guid ClientGroupId,
    string DeactivatedBy,
    string Reason = "Deactivated") : IRequest<Result>;

public class DeactivateClientGroupCommandHandler(
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer)
    : IRequestHandler<DeactivateClientGroupCommand, Result>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(DeactivateClientGroupCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeactivateClientGroupCommandValidator();
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
            var parameters = new DeactivateClientGroupParameters(
                command.ClientGroupId,
                command.DeactivatedBy,
                command.Reason);

            var result = await _clientGroupRepository.DeactivateClientGroupAsync(parameters);

            if (result.Status != RepositoryActionStatus.Updated)
                return Result.Failed("An unexpected error occurred while deactivating the client group. Please try again.");

            var message = _localizer["ClientGroupDeactivatedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to deactivate client group: {ex.Message}");
        }
    }
}

public record DeactivateClientGroupParameters(
    Guid ClientGroupId,
    string DeactivatedBy,
    string Reason = "Deactivated");