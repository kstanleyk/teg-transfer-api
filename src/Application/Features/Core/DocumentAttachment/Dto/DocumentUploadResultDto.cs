namespace TegWallet.Application.Features.Core.DocumentAttachment.Dto;

public record DocumentUploadResultDto
{
    public Guid AttachmentId { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string FileCategory { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }

    // Optional: Add additional properties if needed
    public string? ThumbnailUrl { get; init; }
    public string? PreviewUrl { get; init; }
}