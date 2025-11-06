using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record UpdateClientGroupCommand(
    Guid ClientId,
    string? ClientGroup, // Null to remove from group
    string Reason = "Group updated") : IRequest<Result>;

public class UpdateClientGroupCommandHandler(
    UserManager<Client> userManager,
    IClientGroupRepository clientGroupRepository,
    IAppLocalizer localizer)
    : IRequestHandler<UpdateClientGroupCommand, Result>
{
    public async Task<Result> Handle(UpdateClientGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result.Failed("Client not found");

            ClientGroup? clientGroup = null;

            // If ClientGroup is provided, fetch the ClientGroup entity
            if (!string.IsNullOrWhiteSpace(command.ClientGroup))
            {
                if (!Guid.TryParse(command.ClientGroup, out var clientGroupId))
                    return Result.Failed("Invalid client group ID format");

                clientGroup = await clientGroupRepository.GetAsync(clientGroupId);
                if (clientGroup == null)
                    return Result.Failed("Client group not found");
            }

            // Apply domain logic - UpdateGroup handles both assignment and removal
            client.UpdateGroup(clientGroup, command.Reason);

            // Update using UserManager
            var result = await userManager.UpdateAsync(client);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failed($"Failed to update client group: {errors}");
            }

            return Result.Succeeded(localizer["ClientGroupUpdatedSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}

