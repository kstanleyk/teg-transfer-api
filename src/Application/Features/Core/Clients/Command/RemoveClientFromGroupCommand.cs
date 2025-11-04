using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record RemoveClientFromGroupCommand(
    Guid ClientId,
    string Reason = "Removed from group") : IRequest<Result>;

public class RemoveClientFromGroupCommandHandler(
    UserManager<Client> userManager,
    IAppLocalizer localizer)
    : IRequestHandler<RemoveClientFromGroupCommand, Result>
{
    public async Task<Result> Handle(RemoveClientFromGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await userManager.FindByIdAsync(command.ClientId.ToString());
            if (client == null)
                return Result.Failed("Client not found");

            // Apply domain logic
            client.RemoveFromGroup(command.Reason);

            // Update using UserManager
            var result = await userManager.UpdateAsync(client);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failed($"Failed to remove client from group: {errors}");
            }

            return Result.Succeeded(localizer["ClientRemovedFromGroupSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}