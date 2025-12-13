using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Domain.Entity.Core;

public class DocumentAttachment : Entity<Guid>
{
    // Properties
    public Guid EntityId { get; private init; }
    public string EntityType { get; private init; } = string.Empty;
    public string FileName { get; private init; } = string.Empty;
    public string FileUrl { get; private init; } = string.Empty;
    public string PublicId { get; private init; } = string.Empty;
    public string ContentType { get; private init; } = string.Empty;
    public long FileSize { get; private init; }
    public FileCategory FileCategory { get; private init; }
    public string DocumentType { get; private init; } = string.Empty;
    public string Description { get; private init; } = string.Empty;
    public string UploadedBy { get; private init; } = string.Empty;
    public DateTime UploadedAt { get; private init; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // Private constructor for EF Core
    private DocumentAttachment()
    {
    }

    // Factory method with domain validation
    public static DocumentAttachment Create(Guid entityId, string entityType, string fileName, string fileUrl,
        string publicId, string contentType, long fileSize, string documentType, string description, string uploadedBy)
    {
        DomainGuards.AgainstDefault(entityId);
        DomainGuards.AgainstNullOrWhiteSpace(entityType);
        DomainGuards.AgainstNullOrWhiteSpace(fileName);
        DomainGuards.AgainstNullOrWhiteSpace(fileUrl);
        DomainGuards.AgainstNullOrWhiteSpace(publicId);
        DomainGuards.AgainstNullOrWhiteSpace(contentType);
        DomainGuards.AgainstNullOrWhiteSpace(documentType);
        DomainGuards.AgainstNullOrWhiteSpace(uploadedBy);

        // Validate entity type
        ValidateEntityType(entityType);

        // Validate document type
        ValidateDocumentType(documentType);

        // Validate content type
        ValidateContentType(contentType);

        // Validate content type is allowed for this document type
        ValidateContentTypeForDocumentType(contentType, documentType);

        // Validate file size
        ValidateFileSize(contentType, fileSize);

        // Validate file extension
        ValidateFileExtension(fileName, contentType);

        // Determine file category
        var fileCategory = GetFileCategory(contentType);

        return new DocumentAttachment
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = entityType.Trim(),
            FileName = fileName.Trim(),
            FileUrl = fileUrl.Trim(),
            PublicId = publicId.Trim(),
            ContentType = contentType.Trim(),
            FileSize = fileSize,
            FileCategory = fileCategory,
            DocumentType = documentType.Trim(),
            Description = description.Trim(),
            UploadedBy = uploadedBy.Trim(),
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    // Domain validation methods
    private static void ValidateEntityType(string entityType)
    {
        if (entityType != nameof(Ledger) && entityType != nameof(Reservation))
            throw new DomainException($"Entity type must be either '{nameof(Ledger)}' or '{nameof(Reservation)}'");
    }

    private static void ValidateDocumentType(string documentType)
    {
        if (!AllowedContentTypesByDocumentType.ContainsKey(documentType))
        {
            var validTypes = string.Join(", ", AllowedContentTypesByDocumentType.Keys);
            throw new DomainException($"Invalid document type '{documentType}'. Valid types: {validTypes}");
        }
    }

    private static void ValidateContentType(string contentType)
    {
        if (!AllowedContentTypes.Contains(contentType.ToLower()))
        {
            var supportedTypes = GetSupportedContentTypesDescription();
            throw new DomainException($"Unsupported content type '{contentType}'. {supportedTypes}");
        }
    }

    private static void ValidateContentTypeForDocumentType(string contentType, string documentType)
    {
        if (AllowedContentTypesByDocumentType.TryGetValue(documentType, out var allowedTypes))
        {
            if (!allowedTypes.Contains(contentType.ToLower()))
            {
                var allowedForType = string.Join(", ", GetContentTypeDescriptions(allowedTypes));
                throw new DomainException($"Content type '{contentType}' is not allowed for document type '{documentType}'. Allowed: {allowedForType}");
            }
        }
    }

    private static void ValidateFileSize(string contentType, long fileSize)
    {
        if (fileSize <= 0)
            throw new DomainException("File size must be positive");

        var maxSize = GetMaxFileSize(contentType);
        if (fileSize > maxSize)
        {
            var sizeInMb = maxSize / (1024 * 1024);
            var fileType = GetContentTypeDescription(contentType);
            throw new DomainException($"{fileType} file size must not exceed {sizeInMb}MB");
        }
    }

    private static void ValidateFileExtension(string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new DomainException("File name cannot be empty");

        var extension = Path.GetExtension(fileName).ToLower();
        var allowedExtensions = GetAllowedExtensionsForContentType(contentType);

        if (!allowedExtensions.Contains(extension))
        {
            var allowedExts = string.Join(", ", allowedExtensions);
            throw new DomainException($"File extension '{extension}' is not allowed for content type '{contentType}'. Allowed extensions: {allowedExts}");
        }
    }

    // Helper methods
    private static FileCategory GetFileCategory(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return FileCategory.Image;

        if (contentType == "application/pdf")
            return FileCategory.Pdf;

        if (contentType.StartsWith("video/"))
            return FileCategory.Video;

        return FileCategory.Other;
    }

    private static long GetMaxFileSize(string contentType)
    {
        return GetFileCategory(contentType) switch
        {
            FileCategory.Image => MaxImageSize,
            FileCategory.Pdf => MaxPdfSize,
            FileCategory.Video => MaxVideoSize,
            _ => 5 * 1024 * 1024 // Default 5MB
        };
    }

    private static string[] GetAllowedExtensionsForContentType(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => [".jpg", ".jpeg"],
            "image/png" => [".png"],
            "image/gif" => [".gif"],
            "image/bmp" => [".bmp"],
            "image/webp" => [".webp"],
            "image/tiff" => [".tiff", ".tif"],
            "image/svg+xml" => [".svg"],
            "application/pdf" => [".pdf"],
            "video/mp4" => [".mp4"],
            "video/mpeg" => [".mpeg", ".mpg"],
            "video/quicktime" => [".mov"],
            "video/x-msvideo" => [".avi"],
            "video/x-ms-wmv" => [".wmv"],
            "video/webm" => [".webm"],
            "video/3gpp" => [".3gp", ".3g2"],
            "video/x-matroska" => [".mkv"],
            _ => []
        };
    }

    private static string GetSupportedContentTypesDescription()
    {
        var imageTypes = AllowedContentTypes.Where(c => c.StartsWith("image/"))
            .Select(GetContentTypeDescription);
        var pdfType = AllowedContentTypes.Where(c => c == "application/pdf")
            .Select(GetContentTypeDescription);
        var videoTypes = AllowedContentTypes.Where(c => c.StartsWith("video/"))
            .Select(GetContentTypeDescription);

        var descriptions = imageTypes.Concat(pdfType).Concat(videoTypes);
        return $"Supported types: {string.Join(", ", descriptions)}";
    }

    private static string GetContentTypeDescription(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => "JPEG Image",
            "image/png" => "PNG Image",
            "image/gif" => "GIF Image",
            "image/bmp" => "BMP Image",
            "image/webp" => "WebP Image",
            "image/tiff" => "TIFF Image",
            "image/svg+xml" => "SVG Image",
            "application/pdf" => "PDF Document",
            "video/mp4" => "MP4 Video",
            "video/mpeg" => "MPEG Video",
            "video/quicktime" => "MOV Video",
            "video/x-msvideo" => "AVI Video",
            "video/x-ms-wmv" => "WMV Video",
            "video/webm" => "WebM Video",
            "video/3gpp" => "3GP Video",
            "video/x-matroska" => "MKV Video",
            _ => contentType
        };
    }

    private static IEnumerable<string> GetContentTypeDescriptions(IEnumerable<string> contentTypes)
    {
        return contentTypes.Select(GetContentTypeDescription);
    }

    // Static validation rules - part of the domain
    private static readonly string[] AllowedContentTypes =
    [
        // Images
        "image/jpeg",      // .jpg, .jpeg
        "image/png",       // .png
        "image/gif",       // .gif
        "image/bmp",       // .bmp
        "image/webp",      // .webp
        "image/tiff",      // .tiff, .tif
        "image/svg+xml",   // .svg
        
        // PDF
        "application/pdf", // .pdf
        
        // Video
        "video/mp4",       // .mp4
        "video/mpeg",      // .mpeg, .mpg
        "video/quicktime", // .mov
        "video/x-msvideo", // .avi
        "video/x-ms-wmv",  // .wmv
        "video/webm",      // .webm
        "video/3gpp",      // .3gp, .3g2
        "video/x-matroska" // .mkv
    ];

    private static readonly Dictionary<string, string[]> AllowedContentTypesByDocumentType = new()
    {
        ["ProofOfPayment"] = ["image/jpeg", "image/png", "image/gif", "image/bmp", "application/pdf", "video/mp4", "video/mpeg", "video/quicktime"
        ],
        ["Invoice"] = ["image/jpeg", "image/png", "application/pdf"],
        ["Receipt"] = ["image/jpeg", "image/png", "application/pdf"],
        ["Contract"] = ["image/jpeg", "image/png", "application/pdf"],
        ["IDDocument"] = ["image/jpeg", "image/png", "image/gif", "image/bmp", "application/pdf"],
        ["BankStatement"] = ["image/jpeg", "image/png", "application/pdf"],
        ["VideoProof"] = ["video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/webm"],
        ["Other"] = AllowedContentTypes // Allow all supported types
    };

    // File size limits (in bytes) - part of business rules
    private const long MaxImageSize = 5 * 1024 * 1024;     // 5MB
    private const long MaxPdfSize = 10 * 1024 * 1024;      // 10MB
    private const long MaxVideoSize = 50 * 1024 * 1024;    // 50MB

    // Domain behavior methods
    public void MarkAsDeleted(string deletedBy, string reason)
    {
        DomainGuards.AgainstNullOrWhiteSpace(deletedBy);
        DomainGuards.AgainstNullOrWhiteSpace(reason);

        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy.Trim();
    }

    // Query methods
    public bool IsForLedger => EntityType == nameof(Ledger);
    public bool IsForReservation => EntityType == nameof(Reservation);
    public bool IsImage => FileCategory == FileCategory.Image;
    public bool IsPdf => FileCategory == FileCategory.Pdf;
    public bool IsVideo => FileCategory == FileCategory.Video;
    public string FileExtension => Path.GetExtension(FileName).ToLower();

    // Static helper methods for external use (e.g., in validators or UI)
    public static IReadOnlyList<string> GetAllowedDocumentTypes()
        => AllowedContentTypesByDocumentType.Keys.ToList();

    public static IReadOnlyList<string> GetAllowedContentTypes()
        => AllowedContentTypes.ToList();

    public static IReadOnlyList<string> GetAllowedContentTypesForDocumentType(string documentType)
    {
        return AllowedContentTypesByDocumentType.TryGetValue(documentType, out var allowedTypes)
            ? allowedTypes.ToList()
            : [];
    }

    public static long GetMaximumFileSizeForContentType(string contentType)
        => GetMaxFileSize(contentType);
}

public enum FileCategory
{
    Image = 1,
    Pdf = 2,
    Video = 3,
    Other = 99
}

public enum DocumentType
{
    ProofOfPayment = 1,
    Invoice = 2,
    Contract = 3,
    IdDocument = 4,
    Receipt = 5,
    BankStatement = 6,
    TaxDocument = 7,
    Other = 99
}

public static class DocumentTypeExtensions
{
    private static readonly Dictionary<DocumentType, string[]> AllowedContentTypes = new()
    {
        [DocumentType.ProofOfPayment] = ["image/jpeg", "image/png", "application/pdf"],
        [DocumentType.Invoice] = ["application/pdf", "image/jpeg", "image/png"],
        [DocumentType.Contract] = ["application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        ],
        [DocumentType.IdDocument] = ["image/jpeg", "image/png", "application/pdf"],
        [DocumentType.Receipt] = ["image/jpeg", "image/png", "application/pdf"],
        [DocumentType.BankStatement] = ["application/pdf", "image/jpeg", "image/png"],
        [DocumentType.TaxDocument] = ["application/pdf", "image/jpeg", "image/png"],
        [DocumentType.Other] = ["image/jpeg", "image/png", "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain"
        ]
    };

    public static bool IsContentTypeAllowed(this DocumentType documentType, string contentType)
    {
        return AllowedContentTypes.TryGetValue(documentType, out var allowedTypes)
               && allowedTypes.Contains(contentType.ToLower());
    }

    public static string[] GetAllowedContentTypes(this DocumentType documentType)
    {
        return AllowedContentTypes.TryGetValue(documentType, out var allowedTypes)
            ? allowedTypes
            : [];
    }
}