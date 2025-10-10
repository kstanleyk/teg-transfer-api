using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Core;
using Transfer.Domain.ValueObjects;

namespace Transfer.Infrastructure.Persistence.Configurations.Core;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transaction", SchemaNames.Core);

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                transactionId => transactionId.Value,
                value => new TransactionId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(t => t.Timestamp).IsRequired();
        builder.Property(t => t.Reference).HasMaxLength(100);
        builder.Property(t => t.FailureReason).HasMaxLength(500);
        builder.Property(t => t.Description).HasMaxLength(500);

        // Configure Amount as owned entity
        builder.OwnsOne(t => t.Amount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasColumnName("Amount");

            amountBuilder.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("CurrencyCode");
        });

        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.Timestamp);
        builder.HasIndex(t => new { t.WalletId, t.Timestamp });
    }
}