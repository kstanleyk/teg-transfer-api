using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Infrastructure.Persistence.Configurations.Kyc;

public class KycVerificationHistoryConfiguration : IEntityTypeConfiguration<KycVerificationHistory>
{
    public void Configure(EntityTypeBuilder<KycVerificationHistory> builder)
    {
        builder.ToTable("kyc_verification_history", SchemaNames.Kyc);

        builder.HasKey(h => h.Id);

        builder.Property(h => h.KycProfileId)
            .IsRequired();

        builder.Property(h => h.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(h => h.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(h => h.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.PerformedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(h => h.PerformedAt)
            .IsRequired();

        // Indexes for audit queries
        builder.HasIndex(h => h.KycProfileId);

        builder.HasIndex(h => h.PerformedAt);

        builder.HasIndex(h => h.PerformedBy);

        builder.HasIndex(h => new { h.KycProfileId, h.PerformedAt });
    }
}