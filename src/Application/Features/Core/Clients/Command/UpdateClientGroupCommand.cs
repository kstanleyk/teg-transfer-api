using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record UpdateClientGroupCommand(
    Guid ClientId,
    string? ClientGroupId, // Null to remove from group
    string Reason = "Group updated") : IRequest<Result>;

public class UpdateClientGroupCommandHandler(
    IClientRepository clientRepository,
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer)
    : IRequestHandler<UpdateClientGroupCommand, Result>
{
    public async Task<Result> Handle(UpdateClientGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result.Failed("Client not found");

            var parameters = new UpdateGroupParameters(command.ClientGroupId, command.Reason);

            var result = await clientRepository.UpdateGroupAsync(command.ClientId, parameters);
            if (result.Status!= RepositoryActionStatus.Updated)
            {
                return Result.Failed($"Failed to update client group");
            }

            return Result.Succeeded(localizer["ClientGroupUpdatedSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}

public record UpdateGroupParameters
{
    public string? ClientGroupId { get; init; }
    public string Reason { get; init; } = string.Empty;

    public UpdateGroupParameters(string? clientGroupId, string reason)
    {
        Reason = reason;
        ClientGroupId = clientGroupId;
    }

    public void Validate()
    {
        DomainGuards.AgainstNullOrWhiteSpace(Reason, nameof(Reason));
    }
}

