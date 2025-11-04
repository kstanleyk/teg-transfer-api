using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record AssignClientToGroupCommand(
    Guid ClientId,
    string ClientGroup,
    string Reason = "Group assignment") : IRequest<Result>;

public class AssignClientToGroupCommandHandler(UserManager<Client> userManager)
    : IRequestHandler<AssignClientToGroupCommand, Result>
{
    public async Task<Result> Handle(AssignClientToGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Find client using UserManager
            var client = await userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result.Failed("Client not found");

            // Apply domain logic
            client.AssignToGroup(command.ClientGroup, command.Reason);

            // Update using UserManager
            var result = await userManager.UpdateAsync(client);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failed($"Failed to update client group: {errors}");
            }

            return Result.Succeeded();
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}