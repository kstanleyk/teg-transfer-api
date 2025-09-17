using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class EstateTaskConfiguration : IEntityTypeConfiguration<EstateTask>
{
    public void Configure(EntityTypeBuilder<EstateTask> entity)
    {
        entity.HasKey(e => new { e.Id, e.TaskId, e.EstateId });

        entity.Property(e => e.Id).HasMaxLength(10).IsUnicode(false);
        entity.Property(e => e.TaskId).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.EstateId).HasMaxLength(5).IsUnicode(false);

        entity.ToTable("estate_task", SchemaNames.Core);
    }
}