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
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Timestamp).IsRequired();
        builder.Property(t => t.Reference).HasMaxLength(100);
        builder.Property(t => t.FailureReason).HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(250);

        builder.Property(t => t.CompletionType).HasMaxLength(50).HasDefaultValue(string.Empty);
        builder.Property(t => t.CompletedBy).HasMaxLength(100).HasDefaultValue(string.Empty);
        builder.Property(t => t.CompletedAt).IsRequired(false);

        builder.Property(t => t.ApprovedBy).HasMaxLength(100).HasDefaultValue(string.Empty);
        builder.Property(t => t.ApprovedAt).IsRequired(false);

        builder.Property(t => t.RejectedBy).HasMaxLength(100).HasDefaultValue(string.Empty);
        builder.Property(t => t.RejectedAt).IsRequired(false);

        builder.Property(t => t.ProcessedBy).HasMaxLength(100).HasDefaultValue(string.Empty);
        builder.Property(t => t.ProcessedAt).IsRequired(false);

        // Configure Amount as owned entity
        builder.OwnsOne(t => t.Amount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount).IsRequired().HasColumnType("decimal(18,4)");
            amountBuilder.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .IsRequired().HasMaxLength(3);
        });

        builder.Property(l => l.ReservationId)
            .IsRequired(false).HasDefaultValue(string.Empty);

        // Indexes
        builder.HasIndex(l => l.WalletId);
        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => l.Reference);

        //Index for ReservationId
        builder.HasIndex(l => l.ReservationId);

        // Relationship to Wallet
        builder.HasOne<Wallet>().WithMany(w => w.Ledgers).HasForeignKey(l => l.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        //Optional relationship to Reservation
        builder.HasOne<Reservation>()
            .WithMany() // Reservation doesn't have navigation back to Ledger
            .HasForeignKey(l => l.ReservationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete ledgers when reservation is deleted
    }
}