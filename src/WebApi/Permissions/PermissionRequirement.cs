using Microsoft.AspNetCore.Authorization;

namespace TegWallet.WebApi.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}