using TegWallet.Application.Features.Core.Clients.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IClientRepository:IRepository<Client, Guid>
{
    Task<IReadOnlyList<Client>> GetClientsForExchangeRateQueryAsync();
    Task<Client?> GetClientForExchangeRateQueryAsync(Guid clientId);
    Task<RepositoryActionResult<Client>> RegisterClientAsync(RegisterClientParameters parameters);
    Task<RepositoryActionResult<Client>> RemoveFromGroupAsync(Guid clientId,
        RemoveFromGroupParameters parameters);

    Task<RepositoryActionResult<Client>> AssignToGroupAsync(Guid clientId,
        AssignToGroupParameters parameters);

    Task<RepositoryActionResult<Client>> UpdateGroupAsync(Guid publicId,
        UpdateGroupParameters parameters);

    Task<Client?> GetByEmailAsync(string email);
    Task<Client?> GetByUserIdAsync(Guid userId);
}