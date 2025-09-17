namespace Agrovet.Application.Interfaces.Auth;

public interface IUserPermissionRepository
{
    Task<HashSet<string>> GetPermissionsForUserAsync(string userId);
}