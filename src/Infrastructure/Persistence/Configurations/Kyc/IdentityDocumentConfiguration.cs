using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Infrastructure.Persistence.Configurations.Kyc;

public class IdentityDocumentConfiguration : IEntityTypeConfiguration<IdentityDocument>
{
    public void Configure(EntityTypeBuilder<IdentityDocument> builder)
    {
        builder.ToTable("identity_document", SchemaNames.Kyc);

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ClientId)
            .IsRequired();

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(d => d.DocumentNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.FullName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(d => d.DateOfBirth)
            .IsRequired(false);

        builder.Property(d => d.Nationality)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(d => d.IssueDate)
            .IsRequired();

        builder.Property(d => d.ExpiryDate)
            .IsRequired();

        builder.Property(d => d.IssuingAuthority)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();

        // Document file paths
        builder.Property(d => d.FrontImagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.BackImagePath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(d => d.SelfieImagePath)
            .HasMaxLength(500)
            .IsRequired(false);

        // Value Objects Configuration
        builder.OwnsOne(d => d.VerificationDetails, vd =>
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
        builder.OwnsMany(d => d.Attempts, a =>
        {
            a.WithOwner().HasForeignKey("IdentityDocumentId");
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
        builder.HasIndex(d => d.ClientId);

        //builder.HasIndex(d => new { d.ClientId, d.Type })
        //    .IsUnique()
        //    .HasFilter("[Status] != 'Expired' AND [Status] != 'Rejected'");

        builder.HasIndex(d => d.Type);

        builder.HasIndex(d => d.DocumentNumber);

        builder.HasIndex(d => d.Status);

        builder.HasIndex(d => d.ExpiryDate);

        // Composite index for common queries
        builder.HasIndex(d => new { d.ClientId, d.Status, d.ExpiryDate });
    }
}