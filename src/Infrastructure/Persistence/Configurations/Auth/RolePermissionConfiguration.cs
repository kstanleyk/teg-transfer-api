using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Auth;

namespace TegWallet.Infrastructure.Persistence.Configurations.Auth;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        entity.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .HasPrincipalKey(r => r.Id);

        entity.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .HasPrincipalKey(p => p.Id);

        entity.ToTable("role_permission", SchemaNames.Auth);
    }
}