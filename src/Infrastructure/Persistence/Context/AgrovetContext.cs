using Agrovet.Domain.Entity.Auth;
using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Task = Agrovet.Domain.Entity.Core.Task;

namespace Agrovet.Infrastructure.Persistence.Context;

public class AgrovetContext(DbContextOptions<AgrovetContext> options) : DbContext(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Core
    public virtual DbSet<Task> TaskSet { get; set; }
    public virtual DbSet<Operation> OperationSet { get; set; }
    public virtual DbSet<TaskType> TaskTypeSet { get; set; }
    public virtual DbSet<TaskTypeAccount> TaskTypeAccountSet { get; set; }
    public virtual DbSet<AverageWeight> AverageWeightSet { get; set; }
    public virtual DbSet<Block> BlockSet { get; set; }
    public virtual DbSet<Estate> EstateSet { get; set; }
    public virtual DbSet<ExpenseStatus> ExpenseStatusSet { get; set; }
    public virtual DbSet<ExpenseSource> ExpenseSourceSet { get; set; }
    public virtual DbSet<EstateTask> EstateTaskSet { get; set; }
    public virtual DbSet<EstateTaskType> EstateTaskTypeSet { get; set; }
    public virtual DbSet<HarvestConfig> HarvestConfigSet { get; set; }
    public virtual DbSet<Payroll> PayrollSet { get; set; }
    public virtual DbSet<PayrollAverageWeight> PayrollAverageWeightSet { get; set; }
    public virtual DbSet<Plant> PlantSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgrovetContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}