using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Inventory;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Table name
        builder.ToTable("item", SchemaNames.Inventory);

        // Primary key
        builder.HasKey(i => i.Id);

        // Properties
        builder.Property(i => i.Id).HasMaxLength(10).IsRequired();
        builder.Property(i => i.Name).HasMaxLength(85).IsRequired();
        builder.Property(i => i.ShortDescription).HasMaxLength(250).IsRequired();
        builder.Property(i => i.BarCodeText).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Brand).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Category).HasMaxLength(5).IsRequired();
        builder.Property(i => i.Status).HasMaxLength(5).IsRequired();
        builder.Property(i => i.MinStock).HasColumnType("double precision").IsRequired();
        builder.Property(i => i.MaxStock).HasColumnType("double precision").IsRequired();
        builder.Property(i => i.ReorderLev).HasColumnType("double precision").IsRequired();
        builder.Property(i => i.ReorderQtty).HasColumnType("double precision").IsRequired();
        builder.Property(i => i.PublicId)
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL: generate UUID automatically
        builder.Property(i => i.CreatedOn).IsRequired()
            .HasDefaultValueSql("NOW()"); // PostgreSQL: current UTC timestamp

        // Indexes
        builder.HasIndex(i => i.PublicId).IsUnique();
        builder.HasIndex(i => i.BarCodeText).IsUnique();
    }
}