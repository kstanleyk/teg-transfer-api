using Agrovet.Domain.Entity.Auth;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Context;

public class AgrovetContext(DbContextOptions<AgrovetContext> options) : DbContext(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Inventory
    public virtual DbSet<ItemCategory> ItemCategorySet { get; set; }
    public virtual DbSet<Item> ItemSet { get; set; }
    public virtual DbSet<ItemMovement> ItemMovementSet { get; set; }
    public virtual DbSet<Order> OrderSet { get; set; }
    public virtual DbSet<OrderDetail> OrderDetailSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgrovetContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}