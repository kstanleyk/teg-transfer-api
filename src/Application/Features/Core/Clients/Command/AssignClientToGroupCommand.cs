using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record AssignClientToGroupCommand(
    Guid ClientId,
    Guid ClientGroupId,
    string Reason = "Group assignment") : IRequest<Result>;

public class AssignClientToGroupCommandHandler(
    UserManager<Client> userManager,
    IClientGroupRepository clientGroupRepository, // Assuming you have a repository for ClientGroup
    IAppLocalizer localizer)
    : IRequestHandler<AssignClientToGroupCommand, Result>
{
    public async Task<Result> Handle(AssignClientToGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result.Failed("Client not found");

            var clientGroup = await clientGroupRepository.GetAsync(command.ClientGroupId);
            if (clientGroup == null)
                return Result.Failed("Client group not found");

            // Apply domain logic
            client.AssignToGroup(clientGroup, command.Reason);

            // Update using UserManager
            var result = await userManager.UpdateAsync(client);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failed($"Failed to assign client to group: {errors}");
            }

            return Result.Succeeded(localizer["ClientAssignedToGroupSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}