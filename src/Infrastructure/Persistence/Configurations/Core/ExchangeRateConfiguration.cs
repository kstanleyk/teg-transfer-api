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

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

        builder.Property(x => x.BaseCurrency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TargetCurrency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();

        // Store the actual currency values
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

        builder.Property(x => x.EffectiveFrom).IsRequired();
        builder.Property(x => x.EffectiveTo).IsRequired(false);
        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ClientGroup).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.ClientId).IsRequired(false);
        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.Type, x.IsActive });
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.EffectiveFrom, x.EffectiveTo });
        builder.HasIndex(x => x.ClientId).HasFilter("[ClientId] IS NOT NULL");
        builder.HasIndex(x => x.ClientGroup).HasFilter("[ClientGroup] IS NOT NULL");

        // Constraints
        builder.HasCheckConstraint("CK_ExchangeRates_BaseCurrencyValue", "[BaseCurrencyValue] > 0");
        builder.HasCheckConstraint("CK_ExchangeRates_TargetCurrencyValue", "[TargetCurrencyValue] > 0");
        builder.HasCheckConstraint("CK_ExchangeRates_Margin", "[Margin] >= 0 AND [Margin] <= 1");
    }
}