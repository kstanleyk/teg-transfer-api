using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("wallet", SchemaNames.Core);

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();

        builder.Property(w => w.ClientId)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();

        // Configure BaseCurrency as simple property
        builder.Property(w => w.BaseCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .IsRequired()
            .HasMaxLength(3);

        // Configure Balance as owned entity with explicit column names
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
                .HasColumnName("BalanceCurrency");
        });

        // Configure AvailableBalance as owned entity with explicit column names
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
                .HasColumnName("AvailableBalanceCurrency");
        });

        // Configure relationships - be explicit about foreign keys
        builder.HasMany(w => w.Ledgers)
            .WithOne()
            .HasForeignKey("ClientId") // Use string for shadow property if needed
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Reservations)
            .WithOne()
            .HasForeignKey("ClientId") // Use string for shadow property if needed
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.ClientId)
            .IsUnique();

        // Relationship to Client - make this explicit
        builder.HasOne<Client>()
            .WithOne(c => c.Client)
            .HasForeignKey<Client>(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}