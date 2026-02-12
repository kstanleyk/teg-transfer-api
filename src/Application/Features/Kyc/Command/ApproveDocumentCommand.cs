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

public record ApproveDocumentCommand(
    Guid ClientId,
    Guid DocumentId,
    string ApprovedBy,
    string? Notes = null) : IRequest<Result>;

public class ApproveDocumentCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<ApproveDocumentCommandHandler> logger)
    : IRequestHandler<ApproveDocumentCommand, Result>
{
    public async Task<Result> Handle(ApproveDocumentCommand command, CancellationToken cancellationToken)
    {
        var validator = new ApproveDocumentCommandValidator();
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

            // Business logic: Check if document can be approved
            if (document.Status == KycVerificationStatus.Verified)
                return Result.Failed("Document is already verified.");

            if (document.Status == KycVerificationStatus.Expired)
                return Result.Failed("Cannot approve an expired document. Client must upload a new document.");

            if (document.Status == KycVerificationStatus.Failed)
                return Result.Failed("Cannot approve a failed document. Client must upload a new document.");

            if (document.Status == KycVerificationStatus.Rejected)
                return Result.Failed("Cannot approve a rejected document. Client must upload a new document.");

            if (document.Status == KycVerificationStatus.Pending && !document.FrontImagePath.Any())
                return Result.Failed("Document cannot be approved without a front image.");

            // Business logic: Check if document has expired
            if (document.ExpiryDate < DateTime.UtcNow)
            {
                document.CheckExpiration(); // Update status to expired
                await kycProfileRepository.UpdateDocumentExpiryAsync(command.ClientId, command.DocumentId);
                return Result.Failed("Document has expired. Please ask the client to upload a new document.");
            }

            // Business logic: Check if document is in submitted status or can be directly approved
            if (document.Status != KycVerificationStatus.Submitted)
            {
                logger.LogWarning("Attempted to approve document {DocumentId} that is not in submitted status. Current status: {Status}",
                    command.DocumentId, document.Status);

                // If document is pending but has images, we can submit it first then approve
                if (document.Status == KycVerificationStatus.Pending &&
                    !string.IsNullOrEmpty(document.FrontImagePath))
                {
                    // Auto-submit the document first
                    document.MarkAsSubmitted();
                    logger.LogInformation("Auto-submitted document {DocumentId} before approval", command.DocumentId);
                }
                else
                {
                    return Result.Failed($"Document must be submitted for verification before approval. Current status: {document.Status}");
                }
            }

            var parameters = new ApproveDocumentParameters(
                command.ClientId,
                command.DocumentId,
                command.ApprovedBy,
                command.Notes);

            var result = await kycProfileRepository.ApproveDocumentAsync(parameters, cancellationToken);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                // Update KYC profile status if needed
                //await TryAdvanceKycLevel(kycProfile, command.ApprovedBy, cancellationToken);

                var successMessage = localizer["DocumentApproved"];
                return Result.Succeeded(successMessage);
            }
            else if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("Document not found or cannot be approved.");
            }
            else if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict approving document {DocumentId} for client {ClientId}",
                    command.DocumentId, command.ClientId);
                return Result.Failed("Document was modified by another user. Please refresh and try again.");
            }
            else
            {
                logger.LogError("Repository returned status {Status} for document approval", result.Status);
                return Result.Failed("An error occurred while approving the document.");
            }
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving document {DocumentId} for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result.Failed("An error occurred while approving the document. Please try again.");
        }
    }

    //private async Task TryAdvanceKycLevel(KycProfile kycProfile, string approvedBy, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        // Check if we can advance KYC level
    //        if (kycProfile.HasCompletedLevel2() && kycProfile.Status == KycStatus.Level2Pending)
    //        {
    //            kycProfile.TryAdvanceLevel();
    //            await kycProfileRepository.UpdateAsync(kycProfile, cancellationToken);

    //            logger.LogInformation("KYC profile advanced to {Status} after document approval for client {ClientId}",
    //                kycProfile.Status, kycProfile.ClientId);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log but don't fail the document approval
    //        logger.LogError(ex, "Error advancing KYC level after document approval for client {ClientId}",
    //            kycProfile.ClientId);
    //    }
    //}
}

public record ApproveDocumentParameters(
    Guid ClientId,
    Guid DocumentId,
    string ApprovedBy,
    string? Notes = null);