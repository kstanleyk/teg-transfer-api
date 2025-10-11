using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class LedgerConfiguration : IEntityTypeConfiguration<Ledger>
{
    public void Configure(EntityTypeBuilder<Ledger> builder)
    {
        builder.ToTable("ledger", SchemaNames.Core);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(ledgerId => ledgerId.Value, value => new LedgerId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Timestamp).IsRequired();
        builder.Property(t => t.Reference).HasMaxLength(100);
        builder.Property(t => t.FailureReason).HasMaxLength(500);
        builder.Property(t => t.Description).HasMaxLength(500);

        // Configure Amount as owned entity
        builder.OwnsOne(t => t.Amount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            amountBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.Timestamp);
        builder.HasIndex(t => new { t.WalletId, t.Timestamp });
    }
}