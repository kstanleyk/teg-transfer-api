namespace Agrovet.Domain.Entity.Auth;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public required DateTime CreatedOn { get; set; }

    public string PermissionId { get; set; } = default!;
    public Permission Permission { get; set; } = default!;
}