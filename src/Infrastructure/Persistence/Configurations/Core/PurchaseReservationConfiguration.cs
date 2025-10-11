using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class PurchaseReservationConfiguration : IEntityTypeConfiguration<PurchaseReservation>
{
    public void Configure(EntityTypeBuilder<PurchaseReservation> builder)
    {
        // Table name
        builder.ToTable("purchase_reservations", SchemaNames.Core);

        // Primary Key
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id)
            .ValueGeneratedNever() // Since we generate GUIDs in domain
            .IsRequired();

        // Properties
        builder.Property(pr => pr.ClientId)
            .IsRequired();

        builder.Property(pr => pr.WalletId)
            .IsRequired();

        // LedgerId value objects (stored as GUID)
        builder.Property(pr => pr.PurchaseLedgerId)
            .HasConversion(
                ledgerId => ledgerId.Value,
                value => new LedgerId(value))
            .IsRequired();

        builder.Property(pr => pr.ServiceFeeLedgerId)
            .HasConversion(
                ledgerId => ledgerId.Value,
                value => new LedgerId(value))
            .IsRequired();

        // Money value objects (stored in separate columns)
        builder.OwnsOne(pr => pr.PurchaseAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PurchaseAmount")
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasColumnName("PurchaseCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(pr => pr.ServiceFeeAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ServiceFeeAmount")
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasColumnName("ServiceFeeCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(pr => pr.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 4)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // String properties
        builder.Property(pr => pr.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pr => pr.SupplierDetails)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pr => pr.PaymentMethod)
            .HasMaxLength(100)
            .IsRequired();

        // Status as string (enum)
        builder.Property(pr => pr.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // DateTime properties
        builder.Property(pr => pr.CreatedAt)
            .IsRequired();

        builder.Property(pr => pr.CompletedAt)
            .IsRequired(false);

        builder.Property(pr => pr.CancelledAt)
            .IsRequired(false);

        // Nullable string properties
        builder.Property(pr => pr.CancellationReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(pr => pr.ProcessedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(pr => pr.ClientId);
        builder.HasIndex(pr => pr.WalletId);
        builder.HasIndex(pr => pr.PurchaseLedgerId);
        builder.HasIndex(pr => pr.ServiceFeeLedgerId);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(pr => pr.CreatedAt);
        builder.HasIndex(pr => new { pr.WalletId, pr.Status });

        // Relationships
        builder.HasOne<Wallet>()
            .WithMany(w => w.PurchaseReservations)
            .HasForeignKey(pr => pr.WalletId)
            .OnDelete(DeleteBehavior.Cascade); // Delete reservations when wallet is deleted

        // Note: We don't configure a direct relationship to Ledger entities here
        // because they are managed through the Wallet aggregate
    }
}