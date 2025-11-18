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
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

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

        // Currency Values
        builder.Property(x => x.BaseCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.TargetCurrencyValue)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.Margin)
            .HasPrecision(5, 4)
            .IsRequired();

        // Ignore calculated properties
        builder.Ignore(x => x.MarketRate);
        builder.Ignore(x => x.EffectiveRate);

        // Date and Status
        builder.Property(x => x.EffectiveFrom).IsRequired();
        builder.Property(x => x.EffectiveTo).IsRequired(false);
        builder.Property(x => x.IsActive).IsRequired();

        // Type
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Client targeting
        builder.Property(x => x.ClientId).IsRequired(false);
        builder.Property(x => x.ClientGroupId).IsRequired(false);

        // Audit
        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClientGroup)
            .WithMany(g => g.ExchangeRates)
            .HasForeignKey(x => x.ClientGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Tiers collection (using the backing field)
        builder.HasMany(x => x.Tiers)
            .WithOne(t => t.ExchangeRate)
            .HasForeignKey(t => t.ExchangeRateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map the backing field for Tiers
        builder.Metadata.FindNavigation(nameof(ExchangeRate.Tiers))!
            .SetField("_tiers");
    }
}