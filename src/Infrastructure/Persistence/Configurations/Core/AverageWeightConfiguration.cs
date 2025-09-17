using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class AverageWeightConfiguration : IEntityTypeConfiguration<AverageWeight>
{
    public void Configure(EntityTypeBuilder<AverageWeight> entity)
    {
        entity.HasKey(e => new { e.Id, e.Estate, e.Block });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.Estate).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Block).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Status).HasMaxLength(2).IsUnicode(false);

        entity.ToTable("average_weight", SchemaNames.Core);
    }
}