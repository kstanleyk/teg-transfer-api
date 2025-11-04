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

        // Indexes for Performance

        // Primary index for history lookup by exchange rate
        builder.HasIndex(x => x.ExchangeRateId)
            .HasDatabaseName("IX_ExchangeRateHistories_ExchangeRateId");

        // Index for date-based history queries
        builder.HasIndex(x => x.ChangedAt)
            .HasDatabaseName("IX_ExchangeRateHistories_ChangedAt");

        // Index for administrative queries (who changed what and when)
        builder.HasIndex(x => new { x.ChangedBy, x.ChangedAt })
            .HasDatabaseName("IX_ExchangeRateHistories_ChangedBy_Date");

        // Index for change type analysis
        builder.HasIndex(x => new { x.ChangeType, x.ChangedAt })
            .HasDatabaseName("IX_ExchangeRateHistories_ChangeType_Date");

        // Composite index for efficient history retrieval
        builder.HasIndex(x => new { x.ExchangeRateId, x.ChangedAt })
            .HasDatabaseName("IX_ExchangeRateHistories_RateId_ChangedAt")
            .IsDescending(false, true); // Descending by ChangedAt for latest first

        // Foreign Key Relationship
        builder.HasOne<ExchangeRate>()
            .WithMany() // ExchangeRate doesn't need navigation to history for our use case
            .HasForeignKey(x => x.ExchangeRateId)
            .OnDelete(DeleteBehavior.Cascade) // When rate is deleted, delete its history
            .IsRequired();

        // Check Constraints

        // Ensure positive values in history
        builder.HasCheckConstraint("CK_ExchangeRateHistories_PositiveValues",
            "[PreviousBaseCurrencyValue] > 0 AND [NewBaseCurrencyValue] > 0 AND " +
            "[PreviousTargetCurrencyValue] > 0 AND [NewTargetCurrencyValue] > 0");

        // Ensure valid margins in history
        builder.HasCheckConstraint("CK_ExchangeRateHistories_ValidMargins",
            "[PreviousMargin] >= 0 AND [PreviousMargin] <= 1 AND " +
            "[NewMargin] >= 0 AND [NewMargin] <= 1");

        // Ensure valid rates in history
        builder.HasCheckConstraint("CK_ExchangeRateHistories_ValidRates",
            "[PreviousMarketRate] > 0 AND [NewMarketRate] > 0 AND " +
            "[PreviousEffectiveRate] > 0 AND [NewEffectiveRate] > 0");

        // Comment for documentation
        builder.HasComment("Audit trail for all changes to exchange rates, storing before and after values");
    }
}