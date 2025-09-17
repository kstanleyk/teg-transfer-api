using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = Agrovet.Domain.Entity.Core.Task;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> entity)
    {
        entity.HasKey(e => new { e.Id });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(50).IsUnicode(false);
        entity.Property(e => e.TaskType).HasMaxLength(5).IsUnicode(false);

        entity.ToTable("task", SchemaNames.Core);
    }
}