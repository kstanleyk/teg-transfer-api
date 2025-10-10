using Transfer.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.ValueObjects;

namespace Transfer.Infrastructure.Persistence.Configurations.Core;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallet", SchemaNames.Core);

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();

        builder.Property(w => w.ClientId).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        // Configure BaseCurrency as a simple property with conversion
        builder.Property(w => w.BaseCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .IsRequired()
            .HasMaxLength(3)
            .HasColumnName("BaseCurrencyCode");

        // Configure Balance as owned entity
        builder.OwnsOne(w => w.Balance, balanceBuilder =>
        {
            balanceBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasColumnName("BalanceAmount");

            balanceBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("BalanceCurrencyCode");
        });

        // Configure AvailableBalance as owned entity
        builder.OwnsOne(w => w.AvailableBalance, availableBalanceBuilder =>
        {
            availableBalanceBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasColumnName("AvailableBalanceAmount");

            availableBalanceBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("AvailableBalanceCurrencyCode");
        });

        // Configure transactions relationship
        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.ClientId).IsUnique();
    }
}