using Microsoft.AspNetCore.Authorization;

namespace Transfer.WebApi.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}