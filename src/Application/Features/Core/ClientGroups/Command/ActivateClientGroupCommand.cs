using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ClientGroups.Command;

public record ActivateClientGroupCommand(
    Guid ClientGroupId,
    string ActivatedBy) : IRequest<Result>;

public class ActivateClientGroupCommandHandler(
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer)
    : IRequestHandler<ActivateClientGroupCommand, Result>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result> Handle(ActivateClientGroupCommand command, CancellationToken cancellationToken)
    {
        var validator = new ActivateClientGroupCommandValidator();
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
            var parameters = new ActivateClientGroupParameters(
                command.ClientGroupId,
                command.ActivatedBy);

            var result = await _clientGroupRepository.ActivateClientGroupAsync(parameters);

            if (result.Status != RepositoryActionStatus.Updated)
                return Result.Failed("An unexpected error occurred while activating the client group. Please try again.");

            var message = _localizer["ClientGroupActivatedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failed($"Failed to activate client group: {ex.Message}");
        }
    }
}

public record ActivateClientGroupParameters(
    Guid ClientGroupId,
    string ActivatedBy);