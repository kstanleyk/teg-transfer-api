using MediatR;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Kyc.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Kyc.Command;

public record MarkDocumentUnderReviewCommand(
    Guid ClientId,
    Guid DocumentId,
    string ReviewedBy,
    string? Notes = null) : IRequest<Result>;

public class MarkDocumentUnderReviewCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<MarkDocumentUnderReviewCommandHandler> logger)
    : IRequestHandler<MarkDocumentUnderReviewCommand, Result>
{
    public async Task<Result> Handle(MarkDocumentUnderReviewCommand command, CancellationToken cancellationToken)
    {
        var validator = new MarkDocumentUnderReviewCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        try
        {
            // Get client
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result.Failed($"Client with ID {command.ClientId} not found.");

            // Get KYC profile with documents
            var kycProfile = await kycProfileRepository.GetKycProfileWithDocumentsAsync(command.ClientId);
            if (kycProfile == null)
                return Result.Failed($"KYC profile not found for client ID {command.ClientId}.");

            // Business logic: Check if document exists
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == command.DocumentId);

            if (document == null)
                return Result.Failed($"Document with ID {command.DocumentId} not found for this client.");

            // Business logic: Check if document can be marked under review
            if (document.Status == KycVerificationStatus.Verified)
                return Result.Failed("Document is already verified. Cannot mark as under review.");

            if (document.Status == KycVerificationStatus.Expired)
                return Result.Failed("Document is expired. Cannot mark as under review.");

            if (document.Status == KycVerificationStatus.Rejected)
                return Result.Failed("Document is rejected. Cannot mark as under review.");

            if (document.Status == KycVerificationStatus.Failed)
                return Result.Failed("Document verification has failed. Cannot mark as under review.");

            if (document.Status == KycVerificationStatus.Submitted)
                return Result.Failed("Document is already submitted. Cannot mark as under review.");

            // Check if document has all required images
            if (string.IsNullOrEmpty(document.FrontImagePath))
                return Result.Failed("Document cannot be marked under review without a front image.");

            // Business logic: Check if document is expired
            //if (document.ExpiryDate < DateTime.UtcNow)
            //{
            //    document.CheckExpiration();
            //    await kycProfileRepository.UpdateAsync(kycProfile, cancellationToken);
            //    return Result.Failed("Document has expired. Cannot mark as under review.");
            //}

            var parameters = new MarkDocumentUnderReviewParameters(
                command.ClientId,
                command.DocumentId,
                command.ReviewedBy,
                command.Notes);

            var result = await kycProfileRepository.MarkDocumentUnderReviewAsync(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                // Update KYC profile status if needed
                //await UpdateKycStatusForReview(kycProfile, command.ReviewedBy, cancellationToken);

                var successMessage = localizer["DocumentMarkedUnderReview"];
                return Result.Succeeded(successMessage);
            }

            if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("Document not found or cannot be marked under review.");
            }

            if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict marking document {DocumentId} under review for client {ClientId}",
                    command.DocumentId, command.ClientId);
                return Result.Failed("Document was modified by another user. Please refresh and try again.");
            }

            logger.LogError("Repository returned status {Status} for marking document under review", result.Status);
            return Result.Failed("An error occurred while marking the document under review.");
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking document {DocumentId} under review for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result.Failed("An error occurred while marking the document under review. Please try again.");
        }
    }

    //private async Task UpdateKycStatusForReview(KycProfile kycProfile, string reviewedBy, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        // Update KYC status to Level 2 Pending if not already there
    //        if (kycProfile.Status == KycStatus.Level1Verified)
    //        {
    //            kycProfile.Status = KycStatus.Level2Pending;
    //            await kycProfileRepository.UpdateAsync(kycProfile, cancellationToken);

    //            logger.LogInformation("KYC status updated to Level 2 Pending for client {ClientId} after document marked under review",
    //                kycProfile.ClientId);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log but don't fail the operation
    //        logger.LogError(ex, "Error updating KYC status after marking document under review for client {ClientId}",
    //            kycProfile.ClientId);
    //    }
    //}
}

public record MarkDocumentUnderReviewParameters(
    Guid ClientId,
    Guid DocumentId,
    string ReviewedBy,
    string? Notes = null);