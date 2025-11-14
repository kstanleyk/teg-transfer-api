using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record RemoveClientFromGroupCommand(
    Guid ClientId,
    string Reason = "Removed from group") : IRequest<Result>;

public class RemoveClientFromGroupCommandHandler(
    IClientRepository clientRepository,
    IAppLocalizer localizer)
    : IRequestHandler<RemoveClientFromGroupCommand, Result>
{
    public async Task<Result> Handle(RemoveClientFromGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result.Failed("Client not found");

            // Apply domain logic
            client.RemoveFromGroup(command.Reason);

            var parameters = new RemoveFromGroupParameters(command.Reason);

            // Update using ClientRepository
            var result = await clientRepository.RemoveFromGroupAsync(command.ClientId, parameters);
            if (result.Status != RepositoryActionStatus.Updated)
            {
                return Result.Failed($"Failed to remove client from group");
            }

            return Result.Succeeded(localizer["ClientRemovedFromGroupSuccess"]);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
    }
}

public record RemoveFromGroupParameters
{
    public string Reason { get; init; } = string.Empty;

    public RemoveFromGroupParameters(string reason) => Reason = reason;

    public void Validate() => DomainGuards.AgainstNullOrWhiteSpace(Reason, nameof(Reason));
}