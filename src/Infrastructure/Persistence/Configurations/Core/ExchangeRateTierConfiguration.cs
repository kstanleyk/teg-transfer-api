using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ExchangeRateTierConfiguration : IEntityTypeConfiguration<ExchangeRateTier>
{
    public void Configure(EntityTypeBuilder<ExchangeRateTier> builder)
    {
        builder.ToTable("exchange_rate_tier", SchemaNames.Core);

        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

        // Foreign Key
        builder.Property(x => x.ExchangeRateId).IsRequired();

        // Amount Ranges
        builder.Property(x => x.MinAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.MaxAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        // Rate and Margin
        builder.Property(x => x.Rate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.Margin)
            .HasPrecision(5, 4)
            .IsRequired();

        // Audit
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Indexes for performance
        builder.HasIndex(x => x.ExchangeRateId);
        builder.HasIndex(x => new { x.ExchangeRateId, x.MinAmount, x.MaxAmount });

        // Relationship (already configured in ExchangeRate side)
        builder.HasOne(x => x.ExchangeRate)
            .WithMany(x => x.Tiers)
            .HasForeignKey(x => x.ExchangeRateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}