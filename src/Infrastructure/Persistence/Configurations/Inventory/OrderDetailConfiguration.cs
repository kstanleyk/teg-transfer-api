using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("order_detail", SchemaNames.Inventory);

        // Composite key: OrderId + LineNum
        builder.HasKey(d => new { d.Id, d.LineNum });

        builder.Property(d => d.Id).HasMaxLength(15).IsRequired();
        builder.Property(d => d.LineNum).HasMaxLength(5).IsRequired();
        builder.Property(d => d.Item).HasMaxLength(10).IsRequired();
        builder.Property(d => d.BatchNumber).HasMaxLength(40).IsRequired();
        builder.Property(d => d.Qtty).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.UnitCost).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.Amount).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.Status).HasMaxLength(5).IsRequired();
        builder.Property(d => d.CreatedOn).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(d => d.PublicId).HasDefaultValueSql("gen_random_uuid()");

        // Configure PackagingType as owned entity with nested BottlingType
        builder.OwnsOne(d => d.PackagingType, packagingTypeBuilder =>
        {
            packagingTypeBuilder.Property(pt => pt.Id)
                .HasMaxLength(50)
                .IsRequired();

            // Configure nested BottlingType value object
            packagingTypeBuilder.OwnsOne(pt => pt.BottlingType, bottlingTypeBuilder =>
            {
                bottlingTypeBuilder.Property(bt => bt.SizeInLiters)
                    .HasColumnType("decimal(5,2)") // Stores 5 digits with 2 decimal places
                    .IsRequired();

                bottlingTypeBuilder.Property(bt => bt.DisplayName)
                    .HasMaxLength(10)
                    .IsRequired();
            });

            packagingTypeBuilder.Property(pt => pt.UnitsPerBox)
                .IsRequired();

            packagingTypeBuilder.Property(pt => pt.DisplayName)
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.HasIndex(d => d.PublicId).IsUnique();

        builder.Property(d => d.ExpiryDate);
    }
}