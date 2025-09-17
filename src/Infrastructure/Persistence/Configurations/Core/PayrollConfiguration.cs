using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public virtual void Configure(EntityTypeBuilder<Payroll> entity)
    {
        entity.HasKey(e => new { e.Id });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(100).IsUnicode(false);
        entity.Property(e => e.PayMonth).IsRequired().HasMaxLength(10).IsUnicode(false);
        entity.Property(e => e.PayPeriod).IsRequired().HasMaxLength(100).IsUnicode(false);
        entity.Property(e => e.TransYear).IsRequired().HasMaxLength(10).IsUnicode(false);
        entity.Property(e => e.Remark).IsRequired().HasMaxLength(150).IsUnicode(false);
        entity.Property(e => e.Status).IsRequired().HasMaxLength(5).IsUnicode(false);

        entity.ToTable("payroll", SchemaNames.Core);
    }
}