using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record AssignClientToGroupCommand(
    Guid ClientId,
    Guid ClientGroupId,
    string Reason = "Group assignment") : IRequest<Result>;

public class AssignClientToGroupCommandHandler(
    IClientRepository clientRepository,
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer) : IRequestHandler<AssignClientToGroupCommand, Result>
{
    public async Task<Result> Handle(AssignClientToGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result.Failed("Client not found");

            var clientGroup = await clientGroupRepository.GetAsync(command.ClientGroupId);
            if (clientGroup == null)
                return Result.Failed("Client group not found");

            // Apply domain logic - this now only sets the ID
            client.AssignToGroup(clientGroup, command.Reason);

            var parameters = new AssignToGroupParameters(command.ClientGroupId, command.Reason);

            // Update using ClientRepository
            var result = await clientRepository.AssignToGroupAsync(client.Id,parameters);
            if (result.Status != RepositoryActionStatus.Updated)
            {
                return Result.Failed($"Failed to assign client to group");
            }

            return Result.Succeeded(localizer["ClientAssignedToGroupSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}

public record AssignToGroupParameters
{
    public Guid ClientGroupId { get; init; } 
    public string Reason { get; init; } = string.Empty;

    public AssignToGroupParameters(Guid clientGroupId, string reason)
    {
        Reason = reason;
        ClientGroupId = clientGroupId;
    }

    public void Validate()
    {
        DomainGuards.AgainstNullOrWhiteSpace(Reason, nameof(Reason));
    }
}