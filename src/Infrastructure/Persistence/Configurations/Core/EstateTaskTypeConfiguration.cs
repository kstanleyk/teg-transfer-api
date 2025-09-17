using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class EstateTaskTypeConfiguration : IEntityTypeConfiguration<EstateTaskType>
{
    public void Configure(EntityTypeBuilder<EstateTaskType> entity)
    {
        entity.HasKey(e => new { e.Id, e.TaskTypeId, e.EstateId });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.TaskTypeId).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.AccountId).HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.EstateId).HasMaxLength(5).IsUnicode(false);

        entity.ToTable("estate_task_type", SchemaNames.Core);
    }
}