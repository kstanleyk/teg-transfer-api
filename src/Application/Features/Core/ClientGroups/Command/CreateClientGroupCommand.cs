using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.ClientGroups.Command;

public record CreateClientGroupCommand(
    string Name,
    string Description,
    string CreatedBy) : IRequest<Result<Guid>>;

public class CreateClientGroupCommandHandler(
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer)
    : IRequestHandler<CreateClientGroupCommand, Result<Guid>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IAppLocalizer _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateClientGroupCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateClientGroupCommandValidator();
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
            var parameters = new CreateClientGroupParameters(
                command.Name,
                command.Description,
                command.CreatedBy);

            var result = await _clientGroupRepository.CreateClientGroupAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
                return Result<Guid>.Failed("An unexpected error occurred while creating the client group. Please try again.");

            var message = _localizer["ClientGroupCreatedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, message);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to create client group: {ex.Message}");
        }
    }
}

public record CreateClientGroupParameters(
    string Name,
    string Description,
    string CreatedBy);