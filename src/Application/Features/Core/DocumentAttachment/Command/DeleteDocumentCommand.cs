using MediatR;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Application.Interfaces.Photos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Command;

public record DeleteDocumentCommand(
    Guid EntityId,
    string EntityType, // "Ledger" or "Reservation"
    Guid AttachmentId,
    Guid DeletedBy, // Changed from optional string to required Guid (ClientId)
    string Reason) : IRequest<Result>;

public class DeleteDocumentCommandHandler(
    IDocumentAttachmentRepository documentAttachmentRepository,
    IDocumentService documentService,
    IClientRepository clientRepository,
    IWalletRepository walletRepository,
    IReservationRepository reservationRepository,
    ILedgerRepository ledgerRepository,
    IAppLocalizer localizer,
    ILogger<DeleteDocumentCommandHandler> logger)
    : IRequestHandler<DeleteDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        // Validate command using the validator
        var validator = new DeleteDocumentCommandValidator();
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
            // Log the start of document deletion process
            logger.LogInformation(
                "Starting document deletion. EntityType: {EntityType}, EntityId: {EntityId}, AttachmentId: {AttachmentId}, ClientId: {ClientId}",
                command.EntityType, command.EntityId, command.AttachmentId, command.DeletedBy);

            // 1. Validate client exists and is active
            var client = await clientRepository.GetAsync(command.DeletedBy);
            if (client == null)
            {
                logger.LogWarning("Client not found for document deletion. ClientId: {ClientId}", command.DeletedBy);
                return Result.Failed("Client not found");
            }

            // Check if client's account is active
            if (client.Status != ClientStatus.Active)
            {
                logger.LogWarning(
                    "Client account is not active for document deletion. ClientId: {ClientId}, Status: {Status}",
                    command.DeletedBy, client.Status);
                return Result.Failed("Your account is not active. Please contact support.");
            }

            // 2. Get the attachment first to check if it exists and get Cloudinary public ID
            Domain.Entity.Core.DocumentAttachment? attachment = null;

            if (command.EntityType == nameof(Ledger))
            {
                attachment = await HandleLedgerDocumentDeletion(command, client, cancellationToken);
            }
            else if (command.EntityType == nameof(Reservation))
            {
                attachment = await HandleReservationDocumentDeletion(command, client, cancellationToken);
            }
            else
            {
                logger.LogWarning("Invalid entity type for document deletion: {EntityType}", command.EntityType);
                return Result.Failed($"Invalid entity type. Must be '{nameof(Ledger)}' or '{nameof(Reservation)}'");
            }

            if (attachment == null)
            {
                // The specific handlers should have already logged the reason
                return Result.Failed("Document attachment not found or you don't have permission to delete it");
            }

            if (attachment.IsDeleted)
            {
                logger.LogWarning("Document is already deleted. AttachmentId: {AttachmentId}", attachment.Id);
                return Result.Failed("Document is already deleted");
            }

            // 3. Additional validation: Check if client uploaded this document
            if (attachment.UploadedBy != command.DeletedBy.ToString())
            {
                logger.LogWarning(
                    "Client did not upload this document. ClientId: {ClientId}, UploadedBy: {UploadedBy}, AttachmentId: {AttachmentId}",
                    command.DeletedBy, attachment.UploadedBy, attachment.Id);

                // Option 1: Strict policy - only the uploader can delete
                // return Result.Failed("You can only delete documents that you uploaded");

                // Option 2: Allow deletion if client owns the entity (already checked in permission methods)
                // Continue with deletion
            }

            // 4. Delete from Cloudinary
            var cloudinaryDeleted = await DeleteFromCloudinary(attachment);

            // 5. Remove from database (soft delete)
            await SoftDeleteDocument(command, attachment, cancellationToken);

            // 6. Log the successful deletion
            logger.LogInformation(
                "Document deleted successfully. AttachmentId: {AttachmentId}, EntityType: {EntityType}, " +
                "EntityId: {EntityId}, DeletedBy: {DeletedBy}, CloudinaryDeleted: {CloudinaryDeleted}, Reason: {Reason}",
                command.AttachmentId, command.EntityType, command.EntityId, command.DeletedBy,
                cloudinaryDeleted, command.Reason);

            var message = localizer["DocumentDeletedSuccess"];
            return Result.Succeeded(message);
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain exception in DeleteDocumentCommandHandler");
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error deleting document. EntityId: {EntityId}, EntityType: {EntityType}, " +
                "AttachmentId: {AttachmentId}, DeletedBy: {DeletedBy}",
                command.EntityId, command.EntityType, command.AttachmentId, command.DeletedBy);

            return Result.Failed("An unexpected error occurred while deleting the document");
        }
    }

    private async Task<Domain.Entity.Core.DocumentAttachment?> HandleLedgerDocumentDeletion(
        DeleteDocumentCommand command,
        Client client,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Processing ledger document deletion. LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                command.EntityId, command.AttachmentId);

            // 1. Get the attachment
            var attachment = await documentAttachmentRepository.GetLedgerAttachmentByIdAsync(
                command.EntityId, command.AttachmentId, cancellationToken);

            if (attachment == null)
            {
                logger.LogWarning("Ledger document attachment not found. LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                    command.EntityId, command.AttachmentId);
                return null;
            }

            // 2. Check if ledger exists and get it for validation
            var ledger = await ledgerRepository.GetAsync(command.EntityId);
            if (ledger == null)
            {
                logger.LogWarning("Ledger not found for document deletion. LedgerId: {LedgerId}", command.EntityId);
                return null;
            }

            // 3. Check if ledger is pending
            if (!ledger.IsPending)
            {
                logger.LogWarning(
                    "Cannot delete document from non-pending ledger. LedgerId: {LedgerId}, Status: {Status}",
                    command.EntityId, ledger.Status);
                return null;
            }

            // 4. Check if client has permission to delete from this ledger
            var hasPermission = await ClientHasPermissionToDeleteFromLedgerAsync(
                command.DeletedBy, ledger, attachment, cancellationToken);

            if (!hasPermission)
            {
                logger.LogWarning(
                    "Client does not have permission to delete from ledger. ClientId: {ClientId}, LedgerId: {LedgerId}",
                    command.DeletedBy, command.EntityId);
                return null;
            }

            // 5. Additional validations
            await ValidateLedgerDocumentDeletion(ledger, attachment, cancellationToken);

            return attachment;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ledger document deletion. LedgerId: {LedgerId}, ClientId: {ClientId}",
                command.EntityId, command.DeletedBy);
            throw;
        }
    }

    private async Task<Domain.Entity.Core.DocumentAttachment?> HandleReservationDocumentDeletion(
        DeleteDocumentCommand command,
        Client client,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Processing reservation document deletion. ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                command.EntityId, command.AttachmentId);

            // 1. Get the attachment
            var attachment = await documentAttachmentRepository.GetReservationAttachmentByIdAsync(
                command.EntityId, command.AttachmentId, cancellationToken);

            if (attachment == null)
            {
                logger.LogWarning("Reservation document attachment not found. ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                    command.EntityId, command.AttachmentId);
                return null;
            }

            // 2. Check if reservation exists and get it for validation
            var reservation = await reservationRepository.GetAsync(command.EntityId);
            if (reservation == null)
            {
                logger.LogWarning("Reservation not found for document deletion. ReservationId: {ReservationId}", command.EntityId);
                return null;
            }

            // 3. Check if reservation is pending
            if (!reservation.IsPending)
            {
                logger.LogWarning(
                    "Cannot delete document from non-pending reservation. ReservationId: {ReservationId}, Status: {Status}",
                    command.EntityId, reservation.Status);
                return null;
            }

            // 4. Check if client has permission to delete from this reservation
            var hasPermission = await ClientHasPermissionToDeleteFromReservationAsync(
                command.DeletedBy, reservation, attachment, cancellationToken);

            if (!hasPermission)
            {
                logger.LogWarning(
                    "Client does not have permission to delete from reservation. ClientId: {ClientId}, ReservationId: {ReservationId}",
                    command.DeletedBy, command.EntityId);
                return null;
            }

            // 5. Additional validations
            await ValidateReservationDocumentDeletion(reservation, attachment, cancellationToken);

            return attachment;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling reservation document deletion. ReservationId: {ReservationId}, ClientId: {ClientId}",
                command.EntityId, command.DeletedBy);
            throw;
        }
    }

    private async Task<bool> ClientHasPermissionToDeleteFromLedgerAsync(
        Guid clientId,
        Ledger ledger,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug(
                "Checking client permission for ledger document deletion. ClientId: {ClientId}, LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                clientId, ledger.Id, attachment.Id);

            // 1. Get the wallet that contains this ledger
            var wallet = await walletRepository.GetByLedgerIdAsync(ledger.Id);
            if (wallet == null)
            {
                logger.LogWarning("Wallet not found for ledger. LedgerId: {LedgerId}", ledger.Id);
                return false;
            }

            // 2. Check if the wallet belongs to the client
            if (wallet.ClientId != clientId)
            {
                logger.LogWarning(
                    "Client does not own the wallet containing this ledger. ClientId: {ClientId}, WalletClientId: {WalletClientId}",
                    clientId, wallet.ClientId);
                return false;
            }

            // 3. Check if client uploaded this document (optional but recommended)
            if (attachment.UploadedBy != clientId.ToString())
            {
                logger.LogDebug(
                    "Client did not upload this document, but owns the ledger. ClientId: {ClientId}, UploadedBy: {UploadedBy}",
                    clientId, attachment.UploadedBy);
                // Allow deletion if client owns the ledger, even if they didn't upload the document
            }

            // 4. Check if ledger type allows document deletion
            if (!LedgerTypeAllowsDocumentDeletion(ledger.Type))
            {
                logger.LogWarning("Ledger type does not allow document deletion. LedgerId: {LedgerId}, Type: {Type}",
                    ledger.Id, ledger.Type);
                return false;
            }

            // 5. Check if attachment was created recently (prevent deletion of old documents)
            if (!IsAttachmentDeletionWindowOpen(attachment))
            {
                logger.LogWarning(
                    "Attachment deletion window has expired. AttachmentId: {AttachmentId}, UploadedAt: {UploadedAt}",
                    attachment.Id, attachment.UploadedAt);
                return false;
            }

            logger.LogDebug("Client permission granted for ledger document deletion. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error checking client permission for ledger document deletion. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);
            return false;
        }
    }

    private async Task<bool> ClientHasPermissionToDeleteFromReservationAsync(
        Guid clientId,
        Reservation reservation,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug(
                "Checking client permission for reservation document deletion. ClientId: {ClientId}, ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                clientId, reservation.Id, attachment.Id);

            // 1. Check if the client owns this reservation
            if (reservation.ClientId != clientId)
            {
                logger.LogWarning(
                    "Client does not own this reservation. ClientId: {ClientId}, ReservationClientId: {ReservationClientId}",
                    clientId, reservation.ClientId);
                return false;
            }

            // 2. Check if client uploaded this document (optional but recommended)
            if (attachment.UploadedBy != clientId.ToString())
            {
                logger.LogDebug(
                    "Client did not upload this document, but owns the reservation. ClientId: {ClientId}, UploadedBy: {UploadedBy}",
                    clientId, attachment.UploadedBy);
                // Allow deletion if client owns the reservation, even if they didn't upload the document
            }

            // 3. Check if reservation allows document deletion
            if (!ReservationAllowsDocumentDeletion(reservation))
            {
                logger.LogWarning("Reservation does not allow document deletion. ReservationId: {ReservationId}",
                    reservation.Id);
                return false;
            }

            // 4. Check if attachment was created recently (prevent deletion of old documents)
            if (!IsAttachmentDeletionWindowOpen(attachment))
            {
                logger.LogWarning(
                    "Attachment deletion window has expired. AttachmentId: {AttachmentId}, UploadedAt: {UploadedAt}",
                    attachment.Id, attachment.UploadedAt);
                return false;
            }

            // 5. Check if document is required for compliance (e.g., proof of payment)
            if (IsDocumentRequiredForCompliance(attachment))
            {
                logger.LogWarning(
                    "Document is required for compliance and cannot be deleted. AttachmentId: {AttachmentId}, DocumentType: {DocumentType}",
                    attachment.Id, attachment.DocumentType);
                return false;
            }

            logger.LogDebug("Client permission granted for reservation document deletion. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error checking client permission for reservation document deletion. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);
            return false;
        }
    }

    private async Task ValidateLedgerDocumentDeletion(
        Ledger ledger,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        // Additional validation specific to ledger documents

        // 1. Check if this is the last attachment for the ledger
        var attachmentCount = await documentAttachmentRepository.GetLedgerAttachmentCountAsync(ledger.Id, cancellationToken);

        if (attachmentCount <= 1 && IsDocumentRequiredForLedgerType(ledger.Type))
        {
            logger.LogWarning(
                "Cannot delete the last required document for ledger. LedgerId: {LedgerId}, Type: {Type}, AttachmentCount: {AttachmentCount}",
                ledger.Id, ledger.Type, attachmentCount);
            throw new DomainException("Cannot delete the last required document for this transaction");
        }

        // 2. Check if document has been referenced or used
        if (await IsDocumentReferencedAsync(attachment.Id, cancellationToken))
        {
            logger.LogWarning("Document has been referenced and cannot be deleted. AttachmentId: {AttachmentId}", attachment.Id);
            throw new DomainException("This document has been referenced in other transactions and cannot be deleted");
        }
    }

    private async Task ValidateReservationDocumentDeletion(
        Reservation reservation,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        // Additional validation specific to reservation documents

        // 1. Check if this is the last required document for the reservation
        var attachmentCount = await documentAttachmentRepository.GetReservationAttachmentCountAsync(reservation.Id, cancellationToken);

        if (attachmentCount <= 1)
        {
            logger.LogWarning(
                "Cannot delete the last required document for reservation. ReservationId: {ReservationId}, AttachmentCount: {AttachmentCount}",
                reservation.Id, attachmentCount);
            throw new DomainException("Cannot delete the last required document for this reservation");
        }

        // 2. Check if document type is mandatory for the reservation
        if (IsMandatoryDocumentTypeForReservation(attachment.DocumentType, reservation))
        {
            logger.LogWarning(
                "Cannot delete mandatory document type for reservation. ReservationId: {ReservationId}, DocumentType: {DocumentType}",
                reservation.Id, attachment.DocumentType);
            throw new DomainException("This document type is mandatory and cannot be deleted");
        }
    }

    private async Task<bool> DeleteFromCloudinary(Domain.Entity.Core.DocumentAttachment attachment)
    {
        try
        {
            logger.LogDebug("Deleting document from Cloudinary. PublicId: {PublicId}, AttachmentId: {AttachmentId}",
                attachment.PublicId, attachment.Id);

            var deletionResult = await documentService.DeleteDocument(attachment.PublicId);

            if (deletionResult.Result == "ok")
            {
                logger.LogDebug("Cloudinary deletion successful. PublicId: {PublicId}", attachment.PublicId);
                return true;
            }
            else
            {
                logger.LogWarning(
                    "Failed to delete document from Cloudinary. PublicId: {PublicId}, Result: {Result}, Message: {Message}",
                    attachment.PublicId, deletionResult.Result, deletionResult.Error?.Message);

                // Continue with database deletion even if Cloudinary deletion fails
                // The file will remain in Cloudinary but marked as deleted in our database
                return false;
            }
        }
        catch (Exception cloudEx)
        {
            logger.LogError(cloudEx,
                "Error deleting document from Cloudinary. AttachmentId: {AttachmentId}, PublicId: {PublicId}",
                attachment.Id, attachment.PublicId);

            // Continue with database deletion
            return false;
        }
    }

    private async Task SoftDeleteDocument(
        DeleteDocumentCommand command,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Soft deleting document from database. AttachmentId: {AttachmentId}", attachment.Id);

            if (command.EntityType == nameof(Ledger))
            {
                await documentAttachmentRepository.RemoveLedgerAttachmentAsync(
                    command.EntityId, command.AttachmentId, command.DeletedBy.ToString(), command.Reason, cancellationToken);
            }
            else if (command.EntityType == nameof(Reservation))
            {
                await documentAttachmentRepository.RemoveReservationAttachmentAsync(
                    command.EntityId, command.AttachmentId, command.DeletedBy.ToString(), command.Reason, cancellationToken);
            }

            logger.LogDebug("Document soft deleted successfully. AttachmentId: {AttachmentId}", attachment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error soft deleting document from database. AttachmentId: {AttachmentId}", attachment.Id);
            throw;
        }
    }

    // Helper methods
    private bool LedgerTypeAllowsDocumentDeletion(TransactionType ledgerType)
    {
        // Define which ledger types allow document deletion
        // Some ledger types might not allow deletion once documents are uploaded
        var deletableTypes = new[]
        {
            TransactionType.Deposit,
            TransactionType.Withdrawal,
            TransactionType.Purchase,
            TransactionType.ServiceFee
        };

        return deletableTypes.Contains(ledgerType);
    }

    private bool ReservationAllowsDocumentDeletion(Reservation reservation)
    {
        // Check if reservation allows document deletion
        // Could be based on reservation status, type, age, etc.

        // Example: Don't allow deletion if reservation is about to be processed
        var processingThreshold = TimeSpan.FromHours(1);
        var timeUntilProcessing = reservation.CreatedAt.AddDays(1) - DateTime.UtcNow; // Assuming 24h processing

        if (timeUntilProcessing < processingThreshold)
        {
            logger.LogDebug("Reservation is too close to processing time. TimeUntilProcessing: {Time}",
                timeUntilProcessing);
            return false;
        }

        return true;
    }

    private bool IsAttachmentDeletionWindowOpen(Domain.Entity.Core.DocumentAttachment attachment)
    {
        // Define how long after upload a document can be deleted
        var maxDeletionWindow = TimeSpan.FromHours(24); // 24 hours to delete uploaded documents

        var attachmentAge = DateTime.UtcNow - attachment.UploadedAt;

        if (attachmentAge > maxDeletionWindow)
        {
            logger.LogDebug("Attachment deletion window expired. Age: {Age}, MaxWindow: {MaxWindow}",
                attachmentAge, maxDeletionWindow);
            return false;
        }

        return true;
    }

    private bool IsDocumentRequiredForCompliance(Domain.Entity.Core.DocumentAttachment attachment)
    {
        // Define which document types are required for compliance/audit purposes
        var complianceDocumentTypes = new[]
        {
            "ProofOfPayment",
            "Invoice",
            "Contract",
            "IDDocument",
            "BankStatement"
        };

        return complianceDocumentTypes.Contains(attachment.DocumentType);
    }

    private bool IsDocumentRequiredForLedgerType(TransactionType ledgerType)
    {
        // Define which ledger types require at least one document
        var typesRequiringDocuments = new[]
        {
            TransactionType.Deposit,    // Proof of payment required
            TransactionType.Purchase    // Invoice/receipt required
        };

        return typesRequiringDocuments.Contains(ledgerType);
    }

    private async Task<bool> IsDocumentReferencedAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        // Check if this document has been referenced elsewhere in the system
        // For example, referenced in audit logs, reports, or other transactions

        // This would require additional tracking in your database
        // For now, return false (not implemented)
        return await Task.FromResult(false);
    }

    private bool IsMandatoryDocumentTypeForReservation(string documentType, Reservation reservation)
    {
        // Define which document types are mandatory for reservations
        // Could be based on reservation amount, payment method, etc.

        var mandatoryTypes = new[]
        {
            "ProofOfPayment",
            "Invoice"
        };

        if (mandatoryTypes.Contains(documentType))
        {
            // Check if this document type is specifically required for this reservation
            // For example, high-value reservations might require proof of payment
            var highValueThreshold = 10000m;
            if (reservation.TotalAmount.Amount >= highValueThreshold)
            {
                return true;
            }
        }

        return false;
    }
}