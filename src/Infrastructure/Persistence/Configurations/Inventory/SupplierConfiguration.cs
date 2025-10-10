using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> entity)
    {
        // Table name
        entity.ToTable("supplier", SchemaNames.Inventory);

        // Primary key
        entity.HasKey(i => i.Id);

        entity.Property(e => e.Id).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Name).IsRequired().IsRequired().HasMaxLength(150).IsUnicode(false);
        entity.Property(e => e.Address).IsRequired().HasMaxLength(50).IsUnicode(false);
        entity.Property(e => e.City).IsRequired().HasMaxLength(50).IsUnicode(false);
        entity.Property(e => e.Phone).IsRequired().HasMaxLength(25).IsUnicode(false);
        entity.Property(e => e.ContactPerson).IsRequired().HasMaxLength(100).IsUnicode(false);
    }
}