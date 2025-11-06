using TegWallet.Application.Features.Core.ClientGroups.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IClientGroupRepository : IRepository<ClientGroup, Guid>
{
    Task<ClientGroup?> GetByIdWithClientsAsync(Guid id);

    Task<PagedResult<ClientGroup>> GetAllAsync(bool? isActive = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20);

    Task<IReadOnlyList<ClientGroup>> GetByStatusAsync(bool isActive);
    Task<IReadOnlyList<ClientGroup>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task<RepositoryActionResult<ClientGroup>> CreateClientGroupAsync(
        CreateClientGroupParameters parameters);

    Task<RepositoryActionResult<ClientGroup>> UpdateClientGroupAsync(
        UpdateClientGroupParameters parameters);

    Task<RepositoryActionResult<ClientGroup>> ActivateClientGroupAsync(
        ActivateClientGroupParameters parameters);

    Task<RepositoryActionResult<ClientGroup>> DeactivateClientGroupAsync(
        DeactivateClientGroupParameters parameters);
}