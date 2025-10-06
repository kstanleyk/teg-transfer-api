using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Inventory;

public class ProductMovementConfiguration : IEntityTypeConfiguration<ProductMovement>
{
    public void Configure(EntityTypeBuilder<ProductMovement> builder)
    {
        // Table name
        builder.ToTable("product_movement", SchemaNames.Inventory);

        // Primary key
        builder.HasKey(d => new { d.Id, d.LineNum });

        // Properties
        builder.Property(im => im.Id).HasMaxLength(15).IsRequired();
        builder.Property(im => im.LineNum).HasMaxLength(5).IsRequired();
        builder.Property(im => im.Description).HasMaxLength(500).IsRequired();
        builder.Property(im => im.Item).HasMaxLength(100).IsRequired();
        builder.Property(im => im.TransDate).IsRequired();
        builder.Property(im => im.TransTime).HasMaxLength(10).IsRequired();
        builder.Property(im => im.Sense).HasMaxLength(10).IsRequired();
        builder.Property(im => im.Qtty).HasColumnType("double precision").IsRequired();
        builder.Property(im => im.SourceId).HasMaxLength(100).IsRequired();
        builder.Property(im => im.SourceLineNum).HasMaxLength(50).IsRequired();
        builder.Property(im => im.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(im => im.CreatedOn).IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(im => im.PublicId).IsUnique();
        builder.HasIndex(im => new { im.SourceId, im.LineNum, im.Item })
            .IsUnique(); // Ensures unique movement per item per reference line
    }
}