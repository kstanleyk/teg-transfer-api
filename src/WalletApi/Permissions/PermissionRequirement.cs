using Microsoft.AspNetCore.Authorization;

namespace TegWallet.WalletApi.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}