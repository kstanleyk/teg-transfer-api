using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class TaskTypeAccountConfiguration : IEntityTypeConfiguration<TaskTypeAccount>
{
    public void Configure(EntityTypeBuilder<TaskTypeAccount> entity)
    {
        entity.HasKey(e => new { e.TaskType, e.Estate });


        entity.Property(e => e.TaskType).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Estate).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Account).IsRequired().HasMaxLength(15).IsUnicode(false);

        entity.ToTable("task_type_account", SchemaNames.Core);
    }
}