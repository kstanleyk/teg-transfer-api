using MediatR;
using TegWallet.Application.Interfaces.Auth;

namespace TegWallet.Application.Features.Auth.Permission.Queries;

public record PermissionsQuery : IRequest<string[]>
{
    public required string UserId { get; set; }
}

public class PermissionsQueryHandler(IUserPermissionRepository userPermissionRepository)
    : RequestHandlerBase, IRequestHandler<PermissionsQuery, string[]>
{

    public async Task<string[]> Handle(PermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await userPermissionRepository.GetPermissionsForUserAsync(request.UserId);
        return permissions.ToArray();
    }

    protected override void DisposeCore()
    {
        userPermissionRepository.Dispose();
    }
}