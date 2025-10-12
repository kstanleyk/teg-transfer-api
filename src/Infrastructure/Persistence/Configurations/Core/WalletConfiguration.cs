using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

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
            .HasMaxLength(3);

        // Configure Balance as owned entity
        builder.OwnsOne(w => w.Balance, balanceBuilder =>
        {
            balanceBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            balanceBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3);
        });

        // Configure AvailableBalance as owned entity
        builder.OwnsOne(w => w.AvailableBalance, availableBalanceBuilder =>
        {
            availableBalanceBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            availableBalanceBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3);
        });


        // Configure transactions relationship
        builder.HasMany(w => w.Ledgers)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Reservations (one-to-many)
        builder.HasMany(w => w.Reservations)
            .WithOne()
            .HasForeignKey(pr => pr.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.ClientId).IsUnique();

        // Relationship to Client
        builder.HasOne<Client>()
            .WithOne(c => c.Wallet)
            .HasForeignKey<Wallet>(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}