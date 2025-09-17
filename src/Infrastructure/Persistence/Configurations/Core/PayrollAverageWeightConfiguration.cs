using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class PayrollAverageWeightConfiguration : IEntityTypeConfiguration<PayrollAverageWeight>
{
    public void Configure(EntityTypeBuilder<PayrollAverageWeight> entity)
    {
        entity.HasKey(e => new { e.PayrollId, e.EstateId, e.BlockId });

        entity.Property(e => e.PayrollId).IsRequired().HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.EstateId).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.BlockId).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.AverageFruitBunchWeight).IsRequired().HasMaxLength(5).IsUnicode(false);

        entity.ToTable("payroll_average_weight", SchemaNames.Core);
    }
}