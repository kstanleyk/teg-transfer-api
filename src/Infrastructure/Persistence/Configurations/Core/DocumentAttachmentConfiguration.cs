using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class DocumentAttachmentConfiguration : IEntityTypeConfiguration<DocumentAttachment>
{
    public void Configure(EntityTypeBuilder<DocumentAttachment> builder)
    {
        builder.ToTable("document_attachment", SchemaNames.Core);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Properties
        builder.Property(x => x.EntityId).IsRequired();
        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PublicId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.FileCategory)
            .IsRequired()
            .HasConversion<int>(); // Store the enum as int in database
        // If you want to store it as a string instead, use:
        // .HasConversion<string>()
        // .HasMaxLength(50); // Add appropriate max length if using string

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(x => new { x.EntityId, x.EntityType, x.IsDeleted });
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.UploadedAt);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.UploadedBy);

        // Composite indexes for common queries
        builder.HasIndex(x => new { x.EntityType, x.IsDeleted, x.UploadedAt });
        builder.HasIndex(x => new { x.EntityId, x.EntityType, x.IsDeleted, x.DocumentType });

        //// Check constraint for file size (10MB max)
        //builder.HasCheckConstraint(
        //    "CK_DocumentAttachment_FileSize",
        //    $"[{nameof(DocumentAttachment.FileSize)}] > 0 AND [{nameof(DocumentAttachment.FileSize)}] <= 10485760");

        //// Check constraint for entity type
        //builder.HasCheckConstraint(
        //    "CK_DocumentAttachment_EntityType",
        //    $"[{nameof(DocumentAttachment.EntityType)}] IN ('{nameof(Ledger)}', '{nameof(Reservation)}')");
    }
}