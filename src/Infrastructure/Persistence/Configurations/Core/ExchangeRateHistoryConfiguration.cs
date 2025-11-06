using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
    {
        builder.ToTable("exchange_rate_history", SchemaNames.Core);

        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        // Foreign Key to ExchangeRate
        builder.Property(x => x.ExchangeRateId)
            .IsRequired();

        // Store the actual currency values before and after changes
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

        // Margin changes
        builder.Property(x => x.PreviousMargin)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(x => x.NewMargin)
            .HasPrecision(5, 4)
            .IsRequired();

        // Store calculated rates for historical reference (computed at time of change)
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

        // Audit fields
        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ChangeReason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ChangeType)
            .HasMaxLength(50)
            .IsRequired();

        // Foreign Key Relationship
        builder.HasOne<ExchangeRate>()
            .WithMany() // ExchangeRate doesn't need navigation to history for our use case
            .HasForeignKey(x => x.ExchangeRateId)
            .OnDelete(DeleteBehavior.Cascade) // When rate is deleted, delete its history
            .IsRequired();
    }
}