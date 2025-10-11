namespace TegWallet.Application.Interfaces.Auth;

public interface IUserPermissionRepository: IDisposable
{
    Task<HashSet<string>> GetPermissionsForUserAsync(string userId);
}