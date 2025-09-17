using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class HarvestConfigConfiguration : IEntityTypeConfiguration<HarvestConfig>
{
    public void Configure(EntityTypeBuilder<HarvestConfig> entity)
    {
        entity.HasKey(e => new { e.HarvestId, e.CarryingId });

        entity.Property(e => e.HarvestId).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.CarryingId).IsRequired().HasMaxLength(5).IsUnicode(false);

        entity.ToTable("harvest_config", SchemaNames.Core);
    }
}