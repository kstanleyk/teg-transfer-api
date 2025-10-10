using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouse", SchemaNames.Inventory);

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasMaxLength(10).ValueGeneratedNever(); // We're setting IDs manually with SetId
        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();

        builder.Property(w => w.CreatedOn)
            .IsRequired();

        // Configure Address as an owned entity
        builder.OwnsOne(w => w.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasMaxLength(75);
            addressBuilder.Property(a => a.City).HasMaxLength(100).IsRequired();
            addressBuilder.Property(a => a.State).HasMaxLength(10);
            addressBuilder.Property(a => a.Country).HasMaxLength(50).IsRequired();
            addressBuilder.Property(a => a.ZipCode).HasMaxLength(15);
            addressBuilder.Property(a => a.Quarter).HasMaxLength(100);
            addressBuilder.Property(a => a.Landmark).HasMaxLength(150);
        });

        // Index for better query performance
        builder.HasIndex(w => w.Name);
    }
}