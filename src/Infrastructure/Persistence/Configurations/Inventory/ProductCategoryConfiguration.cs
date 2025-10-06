using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Inventory;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        // Table name
        builder.ToTable("product_category", SchemaNames.Inventory);

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasMaxLength(2).IsRequired();

        // Name property
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();

        // PublicId
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");

        // CreatedOn
        builder.Property(c => c.CreatedOn).IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}