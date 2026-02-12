using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Dto;

public class AttachDocumentToLedgerRequestDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public record DocumentAttachmentDto
{
    public Guid Id { get; init; }
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string PublicId { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string FileCategory { get; init; } = string.Empty; // "Image", "Pdf", "Video"
    public string DocumentType { get; init; } = string.Empty; // "ProofOfPayment", "Invoice", etc.
    public string Description { get; init; } = string.Empty;
    public string UploadedBy { get; init; } = string.Empty; // ClientId as string
    public DateTime UploadedAt { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? DeletedAt { get; init; }
    public string? DeletedBy { get; init; }
    public string? DeletionReason { get; init; }

    // Optional: Add computed properties for UI
    public string FileExtension { get; init; } = string.Empty;
    public string FileSizeFormatted { get; init; } = string.Empty;
    public string UploadedAtFormatted { get; init; } = string.Empty;
}