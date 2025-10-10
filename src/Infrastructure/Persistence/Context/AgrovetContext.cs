using Microsoft.EntityFrameworkCore;
using Transfer.Domain.Abstractions;
using Transfer.Domain.Entity.Auth;
using Transfer.Domain.Entity.Inventory;
using Transfer.Domain.Entity.Sales;

namespace Transfer.Infrastructure.Persistence.Context;

public class AgrovetContext(DbContextOptions<AgrovetContext> options) : DbContext(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Inventory
    public virtual DbSet<Warehouse> WarehouseSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgrovetContext).Assembly);
        // Configure all entities that inherit from Entity<TId>
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Check if this entity type inherits from Entity<T>
            if (IsInheritedFromEntity(clrType))
            {
                var entity = modelBuilder.Entity(clrType);

                // Check if the entity has these properties
                if (entity.Metadata.FindProperty("PublicId") != null)
                {
                    entity.Property("PublicId").HasColumnOrder(1000);
                }

                if (entity.Metadata.FindProperty("CreatedOn") != null)
                {
                    entity.Property("CreatedOn").HasColumnOrder(1001);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static bool IsInheritedFromEntity(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }
}