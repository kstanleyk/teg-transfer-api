using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record UpdateClientGroupCommand(
    Guid ClientId,
    string? ClientGroup, // Null to remove from group
    string Reason = "Group updated") : IRequest<Result>;

public class UpdateClientGroupCommandHandler(
    UserManager<Client> userManager)
    : IRequestHandler<UpdateClientGroupCommand, Result>
{

    public async Task<Result> Handle(UpdateClientGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result.Failed("Client not found");

            // Apply domain logic
            client.UpdateGroup(command.ClientGroup, command.Reason);

            // Update using UserManager
            var result = await userManager.UpdateAsync(client);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failed($"Failed to update client group: {errors}");
            }

            //var message = string.IsNullOrEmpty(command.ClientGroup)
            //    ? _localizer["ClientRemovedFromGroupSuccess"]
            //    : _localizer["ClientAssignedToGroupSuccess", command.ClientGroup];

            return Result.Succeeded();
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}