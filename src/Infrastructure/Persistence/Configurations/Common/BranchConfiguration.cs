using Agrovet.Domain.Entity.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Common;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.HasKey(e => new { e.Id });

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(75).IsUnicode(false);
        entity.Property(e => e.Address).IsRequired().HasMaxLength(50).IsUnicode(false);
        entity.Property(e => e.Telephone).IsRequired().HasMaxLength(35).IsUnicode(false);

        entity.ToTable("branch", SchemaNames.Common);
    }
}