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

public record RejectDocumentCommand(
    Guid ClientId,
    Guid DocumentId,
    string RejectedBy,
    string Reason) : IRequest<Result>;

public class RejectDocumentCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<RejectDocumentCommandHandler> logger)
    : IRequestHandler<RejectDocumentCommand, Result>
{
    public async Task<Result> Handle(RejectDocumentCommand command, CancellationToken cancellationToken)
    {
        var validator = new RejectDocumentCommandValidator();
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

            // Use domain method to reject document (throws DomainException if invalid)
            // This handles all business rules internally
            //document.Reject(command.RejectedBy, command.Reason);

            //// Update KYC profile
            //await kycProfileRepository.UpdateAsync(kycProfile, cancellationToken);

            // Check if this is a critical document for KYC level
            var isCriticalDocument = IsCriticalDocumentForKyc(document.Type);
            if (isCriticalDocument)
            {
                logger.LogInformation("Rejected critical document {DocumentType} for client {ClientId}",
                    document.Type, command.ClientId);

                // Check if this will affect KYC level
                //if (kycProfile.IsLevel2Verified() || kycProfile.IsLevel3Verified())
                //{
                //    // Downgrade KYC level if this document was critical for current level
                //    await DowngradeKycLevelIfNeeded(kycProfile, document, command.RejectedBy, cancellationToken);
                //}
            }

            // Notify client about document rejection (could be async)
            //await NotifyClientAboutRejection(client, document, command.Reason, cancellationToken);

            var successMessage = localizer["DocumentRejected"];
            return Result.Succeeded(successMessage);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting document {DocumentId} for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result.Failed("An error occurred while rejecting the document. Please try again.");
        }
    }

    private bool IsCriticalDocumentForKyc(KycDocumentType documentType)
    {
        return documentType switch
        {
            KycDocumentType.Passport => true,
            KycDocumentType.NationalId => true,
            KycDocumentType.DriversLicense => true,
            KycDocumentType.VotersCard => true,
            KycDocumentType.ProofOfAddress => true,
            _ => false
        };
    }

    //private async Task DowngradeKycLevelIfNeeded(KycProfile kycProfile, IdentityDocument rejectedDocument,
    //    string rejectedBy, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        // Check if this document was critical for the current KYC level
    //        var remainingCriticalDocuments = kycProfile.IdentityDocuments
    //            .Where(d => d.Id != rejectedDocument.Id &&
    //                       d.IsValid &&
    //                       IsCriticalDocumentForKyc(d.Type))
    //            .ToList();

    //        if (!remainingCriticalDocuments.Any())
    //        {
    //            // No valid critical documents left, downgrade KYC level
    //            if (kycProfile.IsLevel3Verified())
    //            {
    //                kycProfile.Status = KycStatus.Level2Pending;
    //                logger.LogInformation("Downgraded KYC from Level 3 to Level 2 pending for client {ClientId} after document rejection",
    //                    kycProfile.ClientId);
    //            }
    //            else if (kycProfile.IsLevel2Verified())
    //            {
    //                kycProfile.Status = KycStatus.Level1Verified;
    //                logger.LogInformation("Downgraded KYC from Level 2 to Level 1 for client {ClientId} after document rejection",
    //                    kycProfile.ClientId);
    //            }

    //            await kycProfileRepository.UpdateAsync(kycProfile, cancellationToken);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log but don't fail the rejection
    //        logger.LogError(ex, "Error downgrading KYC level after document rejection for client {ClientId}",
    //            kycProfile.ClientId);
    //    }
    //}

    //private async Task NotifyClientAboutRejection(Client client, IdentityDocument document, string reason,
    //    CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        logger.LogInformation("Document {DocumentType} ({DocumentNumber}) rejected for client {ClientEmail}. Reason: {Reason}",
    //            document.Type, document.DocumentNumber, client.Email, reason);

    //        // In a real implementation:
    //        // 1. Send email to client
    //        // 2. Create notification in system
    //        // 3. Trigger workflow for document re-upload

    //        await Task.CompletedTask;
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error notifying client about document rejection");
    //    }
    //}
}


public record RejectDocumentParameters(
    Guid ClientId,
    Guid DocumentId,
    string RejectedBy,
    string Reason);