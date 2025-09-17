namespace Agrovet.Domain.Entity.Auth;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public required DateTime CreatedOn { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
}