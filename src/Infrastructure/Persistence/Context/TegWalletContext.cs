using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;
using TegWallet.Infrastructure.Persistence.Configurations;

namespace TegWallet.Infrastructure.Persistence.Context;

public class TegWalletContext(DbContextOptions<TegWalletContext> options)
    : IdentityDbContext<Client, IdentityRole<Guid>, Guid>(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Core
    public virtual DbSet<Wallet> WalletSet { get; set; }
    //public virtual DbSet<Client> ClientSet { get; set; }
    public virtual DbSet<Ledger> LedgerSet { get; set; }
    public DbSet<Reservation> PurchaseReservationSet => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables and assign SchemaNames.Auth
        modelBuilder.Entity<Client>(b =>
        {
            b.ToTable("client", SchemaNames.Identity);

            // Primary Key
            b.HasKey(c => c.Id);

            // Properties
            b.Property(c => c.Email).IsRequired().HasMaxLength(255);
            b.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
            b.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            b.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            b.Property(c => c.CreatedAt).IsRequired();
            b.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            // Indexes
            b.HasIndex(c => c.Email).IsUnique().HasDatabaseName("ix_client_email");
            b.HasIndex(c => c.PhoneNumber).HasDatabaseName("ix_client_phone_number");
            b.HasIndex(c => new { c.FirstName, c.LastName }).HasDatabaseName("ix_client_name");
            b.HasIndex(c => c.Status).HasDatabaseName("ix_client_status");

            // Relationships
            b.HasOne(c => c.Wallet)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdentityRole<Guid>>(b =>
        {
            b.ToTable("role", SchemaNames.Identity);
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("user_role", SchemaNames.Identity);
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("user_claim", SchemaNames.Identity);
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("user_login", SchemaNames.Identity);
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("role_claim", SchemaNames.Identity);
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("user_token", SchemaNames.Identity);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TegWalletContext).Assembly);
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