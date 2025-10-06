using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Inventory;

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
        builder.Property(d => d.Qtty).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.UnitCost).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.Amount).HasColumnType("double precision").IsRequired();
        builder.Property(d => d.Status).HasMaxLength(5).IsRequired();
        builder.Property(d => d.CreatedOn).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(d => d.PublicId).HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(d => d.PublicId).IsUnique();

        builder.Property(d => d.ExpiryDate);
    }
}