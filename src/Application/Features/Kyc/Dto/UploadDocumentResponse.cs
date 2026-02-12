using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Application.Features.Kyc.Dto;

public record UploadDocumentResponse(
    Guid DocumentId,
    Guid ClientId,
    KycDocumentType DocumentType,
    string DocumentNumber,
    string FrontImageUrl,
    string? BackImageUrl,
    DateTime UploadedAt)
{
    public static UploadDocumentResponse FromDomain(IdentityDocument document, string frontImageUrl, string? backImageUrl) =>
        new(document.Id,
            document.ClientId,
            document.Type,
            document.DocumentNumber,
            frontImageUrl,
            backImageUrl,
            document.CreatedAt);
}