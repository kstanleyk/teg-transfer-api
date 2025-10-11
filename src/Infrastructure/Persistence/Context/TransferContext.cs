using Microsoft.EntityFrameworkCore;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Context;

public class TransferContext(DbContextOptions<TransferContext> options) : DbContext(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Core
    public virtual DbSet<Wallet> WalletSet { get; set; }
    public virtual DbSet<Client> ClientSet { get; set; }
    public virtual DbSet<Ledger> LedgerSet { get; set; }
    public DbSet<PurchaseReservation> PurchaseReservationSet => Set<PurchaseReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransferContext).Assembly);
        // Configure all entities that inherit from Entity<TId>
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Check if this entity type inherits from Entity<T>
            if (IsInheritedFromEntity(clrType))
            {
                var entity = modelBuilder.Entity(clrType);

                // Check if the entity has these properties
                if (entity.Metadata.FindProperty("SequentialId") != null)
                {
                    entity.Property("SequentialId").HasColumnOrder(1000);
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