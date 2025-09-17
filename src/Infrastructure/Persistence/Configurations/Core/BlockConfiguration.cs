using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> entity)
    {
        entity.HasKey(e => new { e.Id });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(150).IsUnicode(false);
        entity.Property(e => e.Estate).IsRequired().HasMaxLength(5).IsUnicode(false);

        entity.ToTable("block", SchemaNames.Core);
    }
}