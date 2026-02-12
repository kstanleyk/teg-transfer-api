using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Infrastructure.Persistence.Configurations.Kyc;

public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.ToTable("email_verification", SchemaNames.Kyc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClientId)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.VerifiedAt)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        // Value Objects Configuration
        builder.OwnsOne(e => e.VerificationDetails, vd =>
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
        builder.OwnsMany(e => e.Attempts, a =>
        {
            a.WithOwner().HasForeignKey("EmailVerificationId");
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
        builder.HasIndex(e => e.ClientId)
            .IsUnique();

        builder.HasIndex(e => e.Email);

        builder.HasIndex(e => e.Status);
    }
}