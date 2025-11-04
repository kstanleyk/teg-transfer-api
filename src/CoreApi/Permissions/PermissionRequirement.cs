using Microsoft.AspNetCore.Authorization;

namespace TegWallet.CoreApi.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}