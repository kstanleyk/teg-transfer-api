using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Sales;

namespace Transfer.Infrastructure.Persistence.Configurations.Sales;

public class PriceItemConfiguration : IEntityTypeConfiguration<PriceItem>
{
    public void Configure(EntityTypeBuilder<PriceItem> builder)
    {
        // Table name
        builder.ToTable("price_item", SchemaNames.Sales);

        // Primary key
        builder.HasKey(c => new {c.ChannelId, ProductId = c.ItemId});
        builder.Property(c => c.ChannelId).HasMaxLength(5).IsRequired();
        builder.Property(c => c.ItemId).HasMaxLength(10).IsRequired();
    }
}