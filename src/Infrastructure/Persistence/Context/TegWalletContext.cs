using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;
using TegWallet.Infrastructure.Persistence.Configurations;

namespace TegWallet.Infrastructure.Persistence.Context;

public class TegWalletContext(DbContextOptions<TegWalletContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Core
    public virtual DbSet<Wallet> WalletSet { get; set; }
    public virtual DbSet<Ledger> LedgerSet { get; set; }
    public DbSet<Reservation> PurchaseReservationSet { get; set; }
    public DbSet<ExchangeRate> ExchangeRateSet { get; set; }
    public DbSet<ClientGroup> ClientGroupSet { get; set; }
    public DbSet<RateLock> RateLockSet { get; set; }
    public DbSet<Client> ClientSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables and assign SchemaNames.Auth
        modelBuilder.Entity<Client>(builder =>
        {
            builder.ToTable("client", SchemaNames.Identity);

            // Configure custom properties (Identity properties are handled automatically)
            builder.Property(c => c.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // ClientGroupId relationship
            builder.HasOne(c => c.ClientGroup)
                .WithMany(g => g.Clients)
                .HasForeignKey(c => c.ClientGroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet relationship (one-to-one)
            builder.HasOne(c => c.Wallet)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with ApplicationUser
            builder.HasOne(s => s.User)
                .WithOne(u => u.Client)
                .HasForeignKey<ApplicationUser>(u => u.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
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