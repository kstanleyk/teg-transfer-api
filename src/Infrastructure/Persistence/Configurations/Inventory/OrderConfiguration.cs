using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("order", SchemaNames.Inventory);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasMaxLength(15).IsRequired();
        builder.Property(o => o.OrderType).HasMaxLength(5).IsRequired();
        builder.Property(o => o.Status).HasMaxLength(5).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(500).IsRequired();
        builder.Property(o => o.Supplier).HasMaxLength(15).IsRequired();
        builder.Property(o => o.TransDate).IsRequired();
        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.CreatedOn).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(o => o.PublicId).HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(o => o.PublicId).IsUnique();

        // Configure relationship with OrderDetail
        builder.HasMany(o => o.OrderDetails).WithOne(d => d.Order).HasForeignKey(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade); // Aggregate root deletes cascade
    }
}