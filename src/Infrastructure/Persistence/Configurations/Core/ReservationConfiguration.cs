using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation", SchemaNames.Core);

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).ValueGeneratedNever().IsRequired();

        // Properties
        builder.Property(pr => pr.ClientId).IsRequired();
        builder.Property(pr => pr.WalletId).IsRequired();
        builder.Property(pr => pr.PurchaseLedgerId).IsRequired();
        builder.Property(pr => pr.ServiceFeeLedgerId).IsRequired();

        // Money value objects
        builder.OwnsOne(pr => pr.PurchaseAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(pr => pr.ServiceFeeAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(pr => pr.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .IsRequired();
        });

        // String properties
        builder.Property(pr => pr.Description).HasMaxLength(500).IsRequired();
        builder.Property(pr => pr.SupplierDetails).HasMaxLength(500).IsRequired();
        builder.Property(pr => pr.PaymentMethod).HasMaxLength(100).IsRequired();

        // Status as string (enum)
        builder.Property(pr => pr.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // DateTime properties
        builder.Property(pr => pr.CreatedAt).IsRequired();
        builder.Property(pr => pr.CompletedAt).IsRequired(false);
        builder.Property(pr => pr.CancelledAt).IsRequired(false);

        // Nullable string properties
        builder.Property(pr => pr.CancellationReason).HasMaxLength(1000).IsRequired(false);
        builder.Property(pr => pr.ProcessedBy).HasMaxLength(100).IsRequired(false);

        // Indexes
        builder.HasIndex(pr => pr.ClientId);
        builder.HasIndex(pr => pr.WalletId);
        builder.HasIndex(pr => pr.PurchaseLedgerId);
        builder.HasIndex(pr => pr.ServiceFeeLedgerId);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(pr => pr.CreatedAt);
        builder.HasIndex(pr => new { pr.WalletId, pr.Status });

        // Relationships
        builder.HasOne<Wallet>()
            .WithMany(w => w.Reservations)
            .HasForeignKey(pr => pr.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure attachments collection (same pattern as Ledger)
        builder.Ignore(pr => pr.Attachments); // Navigation property is not directly mapped

        // Optional: Add a shadow property for easier queries if needed
        // builder.Metadata.AddProperty("AttachmentCount", typeof(int));
    }
}