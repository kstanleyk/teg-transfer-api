using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.ToTable("product", SchemaNames.Inventory);

        // Primary Id
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Id).HasMaxLength(10).IsRequired();
        entity.Property(p => p.Name).HasMaxLength(75).IsRequired();
        entity.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        entity.Property(p => p.Category).HasMaxLength(50).IsRequired();
        entity.Property(p => p.Status).HasMaxLength(20).IsRequired();
        entity.Property(p => p.MinStock).IsRequired();
        entity.Property(p => p.MaxStock).IsRequired();
        entity.Property(p => p.ReorderLev).IsRequired();
        entity.Property(p => p.ReorderQtty).IsRequired();

        entity.Property(p => p.PublicId).HasDefaultValueSql("gen_random_uuid()").IsRequired();
        entity.Property(p => p.CreatedOn).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        // --- Value Objects ---
        entity.OwnsOne(product => product.Brand, builder =>
        {
            builder.Property(personalInfo => personalInfo.Name).HasMaxLength(50).IsRequired();
        });

        entity.OwnsOne(product => product.BottlingType, builder =>
        {
            builder.Property(x => x.SizeInLiters).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(5).IsRequired();
        });
    }
}
