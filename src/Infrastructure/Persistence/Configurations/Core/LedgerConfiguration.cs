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

        builder.Property(t => t.CompletionType).HasMaxLength(25).HasDefaultValue(string.Empty);
        builder.Property(t => t.CompletedBy).HasMaxLength(100).HasDefaultValue(string.Empty);
        builder.Property(t => t.CompletedAt).IsRequired(false);
        builder.Property(t => t.ReservationId).IsRequired(false);

        // Configure Amount as owned entity
        builder.OwnsOne(t => t.Amount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount).IsRequired().HasColumnType("decimal(18,4)");
            amountBuilder.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .IsRequired().HasMaxLength(3);
        });

        // Indexes
        builder.HasIndex(l => l.WalletId);
        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => l.Reference);
        builder.HasIndex(l => l.ReservationId);

        // Relationships
        builder.HasOne<Wallet>()
            .WithMany(w => w.Ledgers)
            .HasForeignKey(l => l.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(l => l.ReservationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure attachments collection
        // Note: We don't configure navigation directly because attachments are shared
        // between Ledger and Reservation. Instead, we rely on the DocumentAttachmentConfiguration
        // and query through the EntityId/EntityType pattern.

        // Optional: Add computed property for quick access to attachment count
        builder.Ignore(l => l.Attachments); // Navigation property is not directly mapped

        // If you want to query attachments, use:
        // var attachments = context.DocumentAttachments
        //     .Where(a => a.EntityId == ledgerId && a.EntityType == nameof(Ledger) && !a.IsDeleted)
        //     .ToList();
    }
}