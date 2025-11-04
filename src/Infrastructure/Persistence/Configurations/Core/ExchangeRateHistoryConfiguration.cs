using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
    {
        builder.ToTable("exchange_rate_history", SchemaNames.Core);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

        builder.Property(x => x.ExchangeRateId).IsRequired();

        // Store actual currency values
        builder.Property(x => x.PreviousBaseCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.NewBaseCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.PreviousTargetCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.NewTargetCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.PreviousMargin)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(x => x.NewMargin)
            .HasPrecision(5, 4)
            .IsRequired();

        // Store calculated rates for historical reference
        builder.Property(x => x.PreviousMarketRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.NewMarketRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.PreviousEffectiveRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.NewEffectiveRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.ChangedAt).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChangeReason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ChangeType).HasMaxLength(50).IsRequired();

        // Indexes
        builder.HasIndex(x => x.ExchangeRateId);
        builder.HasIndex(x => x.ChangedAt);
        builder.HasIndex(x => new { x.ExchangeRateId, x.ChangedAt });
    }
}