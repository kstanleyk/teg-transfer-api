using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rate", SchemaNames.Core);

        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        // Currency Properties
        builder.Property(x => x.BaseCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TargetCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();

        // Currency Values (stored in reference currency like USD)
        builder.Property(x => x.BaseCurrencyValue)
            .HasPrecision(18, 8) // 18 total digits, 8 decimal places for precise currency values
            .IsRequired();

        builder.Property(x => x.TargetCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        // Margin
        builder.Property(x => x.Margin)
            .HasPrecision(5, 4) // Up to 99.99% margin (0.9999)
            .IsRequired();

        // Ignore calculated properties (they are computed in the domain)
        builder.Ignore(x => x.MarketRate);
        builder.Ignore(x => x.EffectiveRate);

        // Effective Date Range
        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.EffectiveTo)
            .IsRequired(false);

        // Status and Type
        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>() // Store as string for readability in database
            .HasMaxLength(20)
            .IsRequired();

        // Client Targeting
        builder.Property(x => x.ClientGroup)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.ClientId)
            .IsRequired(false);

        // Audit and Source
        builder.Property(x => x.Source)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Indexes for Performance

        // Primary index for rate resolution (hierarchy: Individual -> Group -> General)
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.Type, x.IsActive, x.EffectiveFrom, x.EffectiveTo })
            .HasDatabaseName("IX_ExchangeRates_RateResolution");

        // Index for active rate lookup by client
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.ClientId, x.IsActive })
            .HasDatabaseName("IX_ExchangeRates_ClientRates")
            .HasFilter("[ClientId] IS NOT NULL AND [IsActive] = 1");

        // Index for active rate lookup by group
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.ClientGroup, x.IsActive })
            .HasDatabaseName("IX_ExchangeRates_GroupRates")
            .HasFilter("[ClientGroup] IS NOT NULL AND [IsActive] = 1");

        // Index for general rates
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.Type, x.IsActive })
            .HasDatabaseName("IX_ExchangeRates_GeneralRates")
            .HasFilter("[Type] = 'General' AND [IsActive] = 1");

        // Index for date-based queries
        builder.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo })
            .HasDatabaseName("IX_ExchangeRates_DateRange");

        // Index for administrative queries
        builder.HasIndex(x => new { x.Type, x.CreatedAt })
            .HasDatabaseName("IX_ExchangeRates_Type_CreatedAt");

        // Check Constraints for Data Integrity

        // Ensure positive currency values
        builder.HasCheckConstraint("CK_ExchangeRates_BaseCurrencyValue", "[BaseCurrencyValue] > 0");
        builder.HasCheckConstraint("CK_ExchangeRates_TargetCurrencyValue", "[TargetCurrencyValue] > 0");

        // Ensure valid margin
        builder.HasCheckConstraint("CK_ExchangeRates_Margin", "[Margin] >= 0 AND [Margin] <= 1");

        // Ensure valid date range
        builder.HasCheckConstraint("CK_ExchangeRates_EffectiveDates",
            "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");

        // Ensure proper rate type constraints
        builder.HasCheckConstraint("CK_ExchangeRates_RateTypeConstraints",
            "([Type] = 'General' AND [ClientId] IS NULL AND [ClientGroup] IS NULL) OR " +
            "([Type] = 'Group' AND [ClientId] IS NULL AND [ClientGroup] IS NOT NULL) OR " +
            "([Type] = 'Individual' AND [ClientId] IS NOT NULL AND [ClientGroup] IS NULL)");

        // Ensure reasonable currency values (prevent data entry errors)
        builder.HasCheckConstraint("CK_ExchangeRates_ReasonableValues",
            "[BaseCurrencyValue] < 1000 AND [TargetCurrencyValue] < 1000");

        // Comment for documentation
        builder.HasComment("Stores exchange rates with hierarchical application: Individual -> Group -> General");
    }
}