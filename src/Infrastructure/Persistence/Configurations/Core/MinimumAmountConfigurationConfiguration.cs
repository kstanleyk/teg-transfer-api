using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class MinimumAmountConfigurationConfiguration : IEntityTypeConfiguration<MinimumAmountConfiguration>
{
    public void Configure(EntityTypeBuilder<MinimumAmountConfiguration> builder)
    {
        builder.ToTable("minimum_amount_configuration", SchemaNames.Core);

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

        // Minimum Amount (in target currency)
        builder.Property(x => x.MinimumAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        // Status and Dates
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.EffectiveFrom).IsRequired();
        builder.Property(x => x.EffectiveTo).IsRequired(false);

        // Audit
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.IsActive });
        builder.HasIndex(x => new { x.IsActive, x.EffectiveFrom, x.EffectiveTo });

        // Unique constraint: Only one active configuration per currency pair at a time
        builder.HasIndex(x => new { x.BaseCurrency, x.TargetCurrency, x.IsActive })
            .HasFilter($"{nameof(MinimumAmountConfiguration.IsActive)} = 1")
            .IsUnique();
    }
}