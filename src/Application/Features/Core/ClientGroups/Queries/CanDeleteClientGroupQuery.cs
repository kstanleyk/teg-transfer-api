using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;

// Check if ClientGroupId can be deleted (no clients assigned)
public record CanDeleteClientGroupQuery(Guid ClientGroupId) : IRequest<Result<bool>>;

public class CanDeleteClientGroupQueryHandler(
    IClientGroupRepository clientGroupRepository)
    : IRequestHandler<CanDeleteClientGroupQuery, Result<bool>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;

    public async Task<Result<bool>> Handle(CanDeleteClientGroupQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var clientGroup = await _clientGroupRepository.GetAsync(query.ClientGroupId);
            if (clientGroup == null)
                return Result<bool>.Failed("Client group not found");

            var canBeDeleted = clientGroup.CanBeDeleted();
            return Result<bool>.Succeeded(canBeDeleted);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failed($"An error occurred while checking if client group can be deleted: {ex.Message}");
        }
    }
}