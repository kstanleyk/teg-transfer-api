using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Infrastructure.Persistence.Configurations.Kyc;

public class PhoneVerificationConfiguration : IEntityTypeConfiguration<PhoneVerification>
{
    public void Configure(EntityTypeBuilder<PhoneVerification> builder)
    {
        builder.ToTable("phone_verification", SchemaNames.Kyc);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClientId)
            .IsRequired();

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.VerifiedAt)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Value Objects Configuration
        builder.OwnsOne(p => p.VerificationDetails, vd =>
        {
            vd.Property(v => v.Method)
                .HasConversion<string>()
                .HasMaxLength(30);

            vd.Property(v => v.VerifiedBy)
                .HasMaxLength(100);

            vd.Property(v => v.VerifiedAt)
                .IsRequired(false);

            vd.Property(v => v.VerificationId)
                .HasMaxLength(100)
                .IsRequired(false);

            vd.Property(v => v.ProviderName)
                .HasMaxLength(50)
                .IsRequired(false);

            vd.Property(v => v.Notes)
                .HasMaxLength(500)
                .IsRequired(false);
        });

        // Collection of verification attempts
        builder.OwnsMany(p => p.Attempts, a =>
        {
            a.WithOwner().HasForeignKey("PhoneVerificationId");
            a.Property<Guid>("Id");
            a.HasKey("Id");

            a.Property(at => at.AttemptedAt)
                .IsRequired();

            a.Property(at => at.Method)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            a.Property(at => at.Successful)
                .IsRequired();

            a.Property(at => at.FailureReason)
                .HasMaxLength(500)
                .IsRequired(false);

            a.Property(at => at.ReferenceId)
                .HasMaxLength(100)
                .IsRequired(false);
        });

        // Indexes
        builder.HasIndex(p => p.ClientId)
            .IsUnique();

        builder.HasIndex(p => p.PhoneNumber);

        builder.HasIndex(p => p.Status);
    }
}