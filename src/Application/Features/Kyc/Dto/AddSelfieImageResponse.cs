using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Application.Features.Kyc.Dto;

public record AddSelfieImageResponse(
    Guid SelfieDocumentId,
    Guid ClientId,
    string SelfieImageUrl,
    DateTime UploadedAt)
{
    public static AddSelfieImageResponse FromDomain(IdentityDocument document, string selfieImageUrl) =>
        new(document.Id,
            document.ClientId,
            selfieImageUrl,
            document.CreatedAt);
}