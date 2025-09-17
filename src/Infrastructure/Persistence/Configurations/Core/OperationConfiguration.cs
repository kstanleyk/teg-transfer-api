using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public virtual void Configure(EntityTypeBuilder<Operation> entity)
    {
        entity.HasKey(e => new { e.Id, e.Line, e.Payroll });

        entity.Property(e => e.Id).HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.Line).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Employee).HasMaxLength(10).IsUnicode(false);
        entity.Property(e => e.Estate).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Block).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Item).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.MillingCycle).HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.Description).HasMaxLength(75).IsUnicode(false);
        entity.Property(e => e.Status).HasMaxLength(2).IsUnicode(false);
        entity.Property(e => e.Payroll).HasMaxLength(15).IsUnicode(false);
        entity.Property(e => e.SyncReference).HasMaxLength(40).IsUnicode(false);

        entity.ToTable("operation", SchemaNames.Core);
    }
}