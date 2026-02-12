using TegWallet.Application.Features.Kyc.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Application.Interfaces.Kyc;

public interface IKycProfileRepository : IRepository<KycProfile, Guid>
{
    Task<RepositoryActionResult<KycProfile>> StartKycProcessAsync(
        StartKycProcessParameters parameters);

    Task<KycProfile?> GetKycProfileByClientIdAsync(Guid clientId);
    Task<KycProfile?> GetKycProfileByClientIdWithEmailVerificationAsync(Guid clientId);
    Task<KycProfile?> GetKycProfileWithDocumentsAsync(Guid clientId);

    Task<RepositoryActionResult<IdentityDocument?>> UpdateUploadDocumentAsync(UploadDocumentParameters parameters);

    Task<RepositoryActionResult<KycProfile?>> SubmitDocumentForVerificationAsync(
        SubmitDocumentForVerificationParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> UpdateDocumentInfoAsync(
        UpdateDocumentInfoParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> DeleteDocumentAsync(
        DeleteDocumentParameters parameters);

    Task<KycProfile?> GetKycProfileWithDocumentAsync(Guid clientId, Guid documentId);

    Task<RepositoryActionResult<IdentityDocument?>> AddDocumentFrontImageAsync(
        AddDocumentFrontImageParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> AddDocumentBackImageAsync(
        AddDocumentBackImageParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> CreateSelfieImageAsync(
        AddSelfieImageParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> UpdateSelfieImageAsync(
        AddSelfieImageParameters parameters);

    Task<KycProfile?> GetByClientIdAsync(Guid clientId);
    Task<RepositoryActionResult<KycProfile?>> SubmitKycForReviewAsync(Guid clientId);

    Task<RepositoryActionResult<IdentityDocument?>> ApproveDocumentAsync(
        ApproveDocumentParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RepositoryActionResult<IdentityDocument?>> UpdateDocumentExpiryAsync(Guid clientId, Guid documentId);

    Task<RepositoryActionResult<IdentityDocument?>> RejectDocumentAsync(RejectDocumentParameters parameters);

    Task<RepositoryActionResult<IdentityDocument?>> MarkDocumentUnderReviewAsync(
        MarkDocumentUnderReviewParameters parameters);

    Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel1Async(ApproveKycLevel1Parameters parameters);
    Task<KycProfile?> GetKycProfileWithVerificationsAsync(Guid clientId);

    Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel2Async(ApproveKycLevel2Parameters parameters);

    Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel3Async(
        ApproveKycLevel3Parameters parameters);
}