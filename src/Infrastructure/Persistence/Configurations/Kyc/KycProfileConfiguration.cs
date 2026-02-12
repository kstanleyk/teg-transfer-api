using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Infrastructure.Persistence.Configurations.Kyc;

public class KycProfileConfiguration : IEntityTypeConfiguration<KycProfile>
{
    public void Configure(EntityTypeBuilder<KycProfile> builder)
    {
        builder.ToTable("kyc_profile", SchemaNames.Kyc);

        builder.HasKey(k => k.Id);

        builder.Property(k => k.ClientId)
            .IsRequired();

        builder.Property(k => k.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(k => k.VerifiedAt)
            .IsRequired(false);

        builder.Property(k => k.ExpiresAt)
            .IsRequired(false);

        builder.Property(k => k.VerificationNotes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(k => k.RejectionReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(k => k.CreatedAt)
            .IsRequired();

        builder.Property(k => k.UpdatedAt)
            .IsRequired();

        builder.Property(k => k.VerifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(k => k.RejectedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Email Verification (one-to-one)
        builder.HasOne(k => k.EmailVerification)
            .WithOne()
            .HasForeignKey<EmailVerification>(e => e.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Phone Verification (one-to-one)
        builder.HasOne(k => k.PhoneVerification)
            .WithOne()
            .HasForeignKey<PhoneVerification>(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Identity Documents (one-to-many)
        builder.HasMany(k => k.IdentityDocuments)
            .WithOne()
            .HasForeignKey(d => d.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Verification History (one-to-many)
        builder.HasMany(k => k.VerificationHistory)
            .WithOne()
            .HasForeignKey(h => h.KycProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(k => k.ClientId)
            .IsUnique();

        builder.HasIndex(k => k.Status);

        builder.HasIndex(k => k.ExpiresAt);

        // Ensure one KYC profile per client
        builder.HasAlternateKey(k => k.ClientId);
    }
}