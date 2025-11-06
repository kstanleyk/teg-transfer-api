using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class RateLockConfiguration : IEntityTypeConfiguration<RateLock>
{
    public void Configure(EntityTypeBuilder<RateLock> builder)
    {
        builder.ToTable("rate_lock", SchemaNames.Core);

        builder.HasKey(rl => rl.Id);
        builder.Property(rl => rl.Id)
            .ValueGeneratedNever()
            .IsRequired();

        // Properties
        builder.Property(rl => rl.ClientId)
            .IsRequired();

        builder.Property(rl => rl.ExchangeRateId)
            .IsRequired();

        // Currency as owned entity or value conversion
        builder.Property(rl => rl.BaseCurrency)
            .HasConversion(
                currency => currency.Code,  // Store as string in DB
                code => Currency.FromCode(code)) // Convert from string to Currency
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(rl => rl.TargetCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(rl => rl.LockedRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(rl => rl.LockedAt)
            .IsRequired();

        builder.Property(rl => rl.ValidUntil)
            .IsRequired();

        builder.Property(rl => rl.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rl => rl.LockReference)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(rl => rl.UsedAt)
            .IsRequired(false);

        // Indexes - updated to use the currency properties correctly
        builder.HasIndex(rl => rl.ClientId)
            .HasDatabaseName("ix_rate_locks_client_id");

        builder.HasIndex(rl => rl.ExchangeRateId)
            .HasDatabaseName("ix_rate_locks_exchange_rate_id");

        builder.HasIndex(rl => new { rl.ClientId, rl.IsUsed, rl.ValidUntil })
            .HasDatabaseName("ix_rate_locks_client_active");

        builder.HasIndex(rl => rl.ValidUntil)
            .HasDatabaseName("ix_rate_locks_valid_until");

        // Index on currency codes (the stored string values)
        builder.HasIndex(rl => new { rl.BaseCurrency, rl.TargetCurrency })
            .HasDatabaseName("ix_rate_locks_currency_pair")
            .HasMethod("btree"); // Or "hash" for exact matches only

        // Composite index for common queries
        builder.HasIndex(rl => new { rl.ClientId, rl.BaseCurrency, rl.TargetCurrency, rl.IsUsed, rl.ValidUntil })
            .HasDatabaseName("ix_rate_locks_client_currency_active");

        // Relationships
        builder.HasOne(rl => rl.Client)
            .WithMany()
            .HasForeignKey(rl => rl.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rl => rl.ExchangeRate)
            .WithMany()
            .HasForeignKey(rl => rl.ExchangeRateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}