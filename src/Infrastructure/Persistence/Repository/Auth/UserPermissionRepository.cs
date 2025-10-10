using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Auth;
using Transfer.Infrastructure.Persistence.Context;

namespace Transfer.Infrastructure.Persistence.Repository.Auth;

public class UserPermissionRepository(AgrovetContext context) :Disposable, IUserPermissionRepository
{
    public async Task<HashSet<string>> GetPermissionsForUserAsync(string userId)
    {
        var usrId = await context.UserSet
            .Where(u => u.IdentityId == userId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var permissions = await context.UserRoleSet
            .Where(ur => ur.UserId == usrId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Id)
            .Distinct()
            .ToListAsync();

        return permissions.ToHashSet();
    }

    //protected override void DisposeCore()
    //{
    //    base.DisposeCore();
    //}
}
