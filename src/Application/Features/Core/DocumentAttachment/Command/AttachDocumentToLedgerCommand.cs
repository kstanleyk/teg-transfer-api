using CloudinaryDotNet.Actions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Core.DocumentAttachment.Dto;
using TegWallet.Application.Features.Core.DocumentAttachment.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Application.Interfaces.Photos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Command;

public record AttachDocumentToLedgerCommand(
    Guid ClientId,
    Guid LedgerId,
    IFormFile File,
    string DocumentType,
    string? Description = null) : IRequest<Result<DocumentUploadResultDto>>;

public class AttachDocumentToLedgerCommandHandler(
    ILedgerRepository ledgerRepository,
    IDocumentService documentService,
    IDocumentAttachmentRepository documentAttachmentRepository,
    IClientRepository clientRepository,
    IWalletRepository walletRepository,
    IAppLocalizer localizer,
    ILogger<AttachDocumentToLedgerCommandHandler> logger)
    : IRequestHandler<AttachDocumentToLedgerCommand, Result<DocumentUploadResultDto>>
{
    public async Task<Result<DocumentUploadResultDto>> Handle(
        AttachDocumentToLedgerCommand command,
        CancellationToken cancellationToken)
    {
        // Validate command using the validator
        var validator = new AttachDocumentToLedgerCommandValidator();
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
            // Log the start of document attachment process
            logger.LogInformation(
                "Starting document attachment to ledger. LedgerId: {LedgerId}, ClientId: {ClientId}, DocumentType: {DocumentType}, FileName: {FileName}",
                command.LedgerId, command.ClientId, command.DocumentType, command.File.FileName);

            // 1. Validate client exists
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
            {
                logger.LogWarning("Client not found for ledger document upload. ClientId: {ClientId}", command.ClientId);
                return Result<DocumentUploadResultDto>.Failed("Client not found");
            }

            // 2. Validate client is active
            if (client.Status != ClientStatus.Active)
            {
                logger.LogWarning("Client is not active. ClientId: {ClientId}, Status: {Status}",
                    command.ClientId, client.Status);
                return Result<DocumentUploadResultDto>.Failed("Client account is not active");
            }

            // 3. Validate ledger exists
            var ledger = await ledgerRepository.GetAsync(command.LedgerId);
            if (ledger == null)
            {
                logger.LogWarning("Ledger not found. LedgerId: {LedgerId}", command.LedgerId);
                return Result<DocumentUploadResultDto>.Failed("Ledger not found");
            }

            // 4. Check if ledger is pending
            if (!ledger.IsPending)
            {
                logger.LogWarning(
                    "Cannot attach document to non-pending ledger. LedgerId: {LedgerId}, Status: {Status}",
                    command.LedgerId, ledger.Status);
                return Result<DocumentUploadResultDto>.Failed("Cannot attach document to a ledger that is not in pending status");
            }

            // 5. Check if client has permission to upload to this ledger
            var hasPermission = await ClientHasPermissionToUploadAsync(command.ClientId, ledger);
            if (!hasPermission)
            {
                logger.LogWarning(
                    "Client does not have permission to upload to ledger. ClientId: {ClientId}, LedgerId: {LedgerId}",
                    command.ClientId, command.LedgerId);
                return Result<DocumentUploadResultDto>.Failed("Client does not have permission to upload documents for this ledger");
            }

            // 6. Upload to Cloudinary
            UploadResult? uploadResult;
            try
            {
                logger.LogDebug("Uploading file to Cloudinary. FileName: {FileName}, Size: {Size} bytes, ContentType: {ContentType}",
                    command.File.FileName, command.File.Length, command.File.ContentType);

                uploadResult = await documentService.UploadDocument(command.File);

                if (uploadResult == null)
                {
                    logger.LogError("Cloudinary upload returned null result for file: {FileName}", command.File.FileName);
                    return Result<DocumentUploadResultDto>.Failed("Failed to upload document to cloud storage");
                }

                logger.LogDebug("Cloudinary upload completed. PublicId: {PublicId}, StatusCode: {StatusCode}, Format: {Format}",
                    uploadResult.PublicId, uploadResult.StatusCode, uploadResult.Format);
            }
            catch (Exception uploadEx)
            {
                logger.LogError(uploadEx,
                    "Cloudinary upload failed. LedgerId: {LedgerId}, ClientId: {ClientId}, FileName: {FileName}",
                    command.LedgerId, command.ClientId, command.File.FileName);

                return Result<DocumentUploadResultDto>.Failed(
                    "Failed to upload document to cloud storage. Please try again or contact support.");
            }

            // 7. Validate Cloudinary upload result
            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                logger.LogWarning(
                    "Cloudinary upload returned non-OK status. StatusCode: {StatusCode}, Error: {ErrorMessage}",
                    uploadResult.StatusCode, uploadResult.Error?.Message);

                var errorMessage = GetUserFriendlyErrorMessage(uploadResult.Error?.Message);
                return Result<DocumentUploadResultDto>.Failed(errorMessage);
            }

            // 8. Check for specific Cloudinary errors
            if (!string.IsNullOrEmpty(uploadResult.Error?.Message))
            {
                var cloudinaryError = uploadResult.Error.Message.ToLower();
                var userMessage = GetUserFriendlyErrorMessage(cloudinaryError);

                logger.LogWarning("Cloudinary error: {Error}, UserMessage: {UserMessage}",
                    cloudinaryError, userMessage);

                return Result<DocumentUploadResultDto>.Failed(userMessage);
            }

            // 9. Validate that we got a secure URL
            if (string.IsNullOrEmpty(uploadResult.SecureUrl?.AbsoluteUri))
            {
                logger.LogError("Cloudinary upload succeeded but returned no secure URL. PublicId: {PublicId}",
                    uploadResult.PublicId);
                return Result<DocumentUploadResultDto>.Failed("Document upload completed but failed to get file URL");
            }

            // 10. Create document attachment entity (DOMAIN VALIDATION HAPPENS HERE)
            Domain.Entity.Core.DocumentAttachment attachment;
            try
            {
                attachment = Domain.Entity.Core.DocumentAttachment.Create(
                    entityId: command.LedgerId,
                    entityType: nameof(Ledger),
                    fileName: command.File.FileName,
                    fileUrl: uploadResult.SecureUrl.AbsoluteUri,
                    publicId: uploadResult.PublicId,
                    contentType: command.File.ContentType,
                    fileSize: command.File.Length,
                    documentType: command.DocumentType,
                    description: command.Description ?? string.Empty,
                    uploadedBy: command.ClientId.ToString());

                logger.LogDebug("Document attachment entity created. AttachmentId: {AttachmentId}, FileCategory: {FileCategory}",
                    attachment.Id, attachment.FileCategory);
            }
            catch (DomainException domainEx)
            {
                logger.LogError(domainEx, "Domain validation failed while creating attachment");
                return Result<DocumentUploadResultDto>.Failed(domainEx.Message);
            }

            // 11. Save to database through repository
            Domain.Entity.Core.DocumentAttachment savedAttachment;
            try
            {
                savedAttachment = await documentAttachmentRepository.AddLedgerAttachmentAsync(
                    command.LedgerId, attachment, cancellationToken);

                logger.LogDebug("Document attachment saved to database. AttachmentId: {AttachmentId}",
                    savedAttachment.Id);
            }
            catch (Exception dbEx)
            {
                logger.LogError(dbEx,
                    "Failed to save document attachment to database. LedgerId: {LedgerId}, PublicId: {PublicId}",
                    command.LedgerId, uploadResult.PublicId);

                // Attempt to delete from Cloudinary since database save failed
                try
                {
                    await documentService.DeleteDocument(uploadResult.PublicId);
                    logger.LogInformation("Cleaned up Cloudinary file after database save failure. PublicId: {PublicId}",
                        uploadResult.PublicId);
                }
                catch (Exception cleanupEx)
                {
                    logger.LogError(cleanupEx,
                        "Failed to clean up Cloudinary file after database save failure. PublicId: {PublicId}",
                        uploadResult.PublicId);
                }

                return Result<DocumentUploadResultDto>.Failed(
                    "Failed to save document metadata. Please try again.");
            }

            // 12. Update ledger with attachment
            try
            {
                ledger.AddExistingAttachment(savedAttachment);
                //await _ledgerRepository.UpdateAsync(ledger, cancellationToken);

                logger.LogDebug("Ledger updated with new attachment. LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                    command.LedgerId, savedAttachment.Id);
            }
            catch (Exception updateEx)
            {
                // Log but don't fail the operation since the attachment is already saved
                logger.LogWarning(updateEx,
                    "Failed to update ledger with attachment reference. LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                    command.LedgerId, savedAttachment.Id);
            }

            // 13. Create success response DTO
            var resultDto = new DocumentUploadResultDto
            {
                AttachmentId = savedAttachment.Id,
                FileUrl = savedAttachment.FileUrl,
                FileName = savedAttachment.FileName,
                DocumentType = savedAttachment.DocumentType,
                UploadedAt = savedAttachment.UploadedAt,
                ContentType = savedAttachment.ContentType,
                FileSize = savedAttachment.FileSize,
                FileCategory = savedAttachment.FileCategory.ToString()
            };

            // 14. Log successful completion
            logger.LogInformation(
                "Document attached to ledger successfully. LedgerId: {LedgerId}, ClientId: {ClientId}, " +
                "AttachmentId: {AttachmentId}, FileType: {ContentType}, FileSize: {FileSize} bytes",
                command.LedgerId, command.ClientId, savedAttachment.Id,
                savedAttachment.ContentType, savedAttachment.FileSize);

            var message = localizer["DocumentUploadedSuccess"];
            return Result<DocumentUploadResultDto>.Succeeded(resultDto, message);
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain exception in AttachDocumentToLedgerCommandHandler");
            return Result<DocumentUploadResultDto>.Failed(ex.Message);
        }
        catch (ValidationException ex)
        {
            // This should have been caught earlier, but just in case
            logger.LogWarning(ex, "Validation exception in AttachDocumentToLedgerCommandHandler");
            return Result<DocumentUploadResultDto>.Failed(string.Join(", ", ex.ValidationErrors));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error in AttachDocumentToLedgerCommandHandler. LedgerId: {LedgerId}, ClientId: {ClientId}",
                command.LedgerId, command.ClientId);

            return Result<DocumentUploadResultDto>.Failed(
                "An unexpected error occurred while attaching the document. Please try again.");
        }
    }

    //private async Task<bool> ClientHasPermissionToUploadAsync(Guid clientId, Ledger ledger)
    //{
    //    try
    //    {
    //        // Get the wallet that contains this ledger
    //        var wallet = await _walletRepository.GetByLedgerIdAsync(ledger.Id);
    //        if (wallet == null)
    //        {
    //            _logger.LogWarning("Wallet not found for ledger. LedgerId: {LedgerId}", ledger.Id);
    //            return false;
    //        }

    //        // Check if the wallet belongs to the client
    //        if (wallet.ClientId != clientId)
    //        {
    //            _logger.LogWarning(
    //                "Client does not own the wallet containing this ledger. ClientId: {ClientId}, WalletClientId: {WalletClientId}",
    //                clientId, wallet.ClientId);
    //            return false;
    //        }

    //        // Additional checks if needed:
    //        // - Check if client's account is not suspended
    //        // - Check if wallet is active
    //        // - Check if ledger type allows document attachments

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex,
    //            "Error checking client permission for ledger. ClientId: {ClientId}, LedgerId: {LedgerId}",
    //            clientId, ledger.Id);
    //        return false;
    //    }
    //}

    private async Task<bool> ClientHasPermissionToUploadAsync(
    Guid clientId,
    Ledger ledger)
    {
        try
        {
            logger.LogDebug(
                "Checking client permission for ledger upload. ClientId: {ClientId}, LedgerId: {LedgerId}, LedgerType: {LedgerType}",
                clientId, ledger.Id, ledger.Type);

            // 1. Get the wallet that contains this ledger
            var wallet = await walletRepository.GetByLedgerIdAsync(ledger.Id);
            if (wallet == null)
            {
                logger.LogWarning("Wallet not found for ledger. LedgerId: {LedgerId}", ledger.Id);
                return false;
            }

            logger.LogDebug("Found wallet {WalletId} for ledger {LedgerId}", wallet.Id, ledger.Id);

            // 2. Check if the wallet belongs to the client
            if (wallet.ClientId != clientId)
            {
                logger.LogWarning(
                    "Client does not own the wallet containing this ledger. ClientId: {ClientId}, WalletClientId: {WalletClientId}",
                    clientId, wallet.ClientId);
                return false;
            }

            logger.LogDebug("Client {ClientId} owns wallet {WalletId}", clientId, wallet.Id);

            // 3. Get client to check account status
            var client = await clientRepository.GetAsync(clientId);
            if (client == null)
            {
                logger.LogWarning("Client not found during permission check. ClientId: {ClientId}", clientId);
                return false;
            }

            // 4. Check if client's account is not suspended
            if (client.Status == ClientStatus.Suspended)
            {
                logger.LogWarning(
                    "Client account is suspended. ClientId: {ClientId}, Status: {Status}",
                    clientId, client.Status);
                return false;
            }

            // 5. Check if client's account is active (not inactive)
            if (client.Status == ClientStatus.Inactive)
            {
                logger.LogWarning(
                    "Client account is inactive. ClientId: {ClientId}, Status: {Status}",
                    clientId, client.Status);
                return false;
            }

            logger.LogDebug("Client {ClientId} account status is valid: {Status}", clientId, client.Status);

            // 6. Check if wallet is "active" (has positive balance or is in good standing)
            // Since we don't have an explicit "WalletStatus", we can check:
            // a. Wallet has been created and is not locked
            // b. Wallet balance is not negative (if that's a business rule)
            // c. Wallet has been used recently (optional)

            if (!IsWalletActive(wallet))
            {
                logger.LogWarning("Wallet is not active. WalletId: {WalletId}", wallet.Id);
                return false;
            }

            logger.LogDebug("Wallet {WalletId} is active", wallet.Id);

            // 7. Check if ledger type allows document attachments
            if (!LedgerTypeAllowsDocumentAttachments(ledger.Type))
            {
                logger.LogWarning(
                    "Ledger type does not allow document attachments. LedgerId: {LedgerId}, Type: {Type}",
                    ledger.Id, ledger.Type);
                return false;
            }

            logger.LogDebug("Ledger type {LedgerType} allows document attachments", ledger.Type);

            // 8. Additional check: Verify ledger amount is positive (if applicable)
            if (ledger.Amount.Amount <= 0)
            {
                logger.LogWarning(
                    "Ledger amount is not positive. LedgerId: {LedgerId}, Amount: {Amount}",
                    ledger.Id, ledger.Amount.Amount);
                return false;
            }

            // 9. Check if document attachment limit hasn't been exceeded for this ledger
            var attachmentCount = await documentAttachmentRepository.GetLedgerAttachmentCountAsync(ledger.Id);
            var maxAttachments = GetMaxAttachmentsForLedgerType(ledger.Type);

            if (attachmentCount >= maxAttachments)
            {
                logger.LogWarning(
                    "Document attachment limit exceeded for ledger. LedgerId: {LedgerId}, Current: {Current}, Max: {Max}",
                    ledger.Id, attachmentCount, maxAttachments);
                return false;
            }

            logger.LogDebug("Attachment count within limits. LedgerId: {LedgerId}, Count: {Count}, Max: {Max}",
                ledger.Id, attachmentCount, maxAttachments);

            // 10. Check if client has exceeded daily upload limit
            var todayUploads = await GetClientDailyUploadCountAsync(clientId);
            var dailyLimit = GetDailyUploadLimit();

            if (todayUploads >= dailyLimit)
            {
                logger.LogWarning(
                    "Client daily upload limit exceeded. ClientId: {ClientId}, Today: {Today}, Limit: {Limit}",
                    clientId, todayUploads, dailyLimit);
                return false;
            }

            logger.LogDebug("Client daily upload count: {Today}/{Limit}", todayUploads, dailyLimit);

            // All checks passed
            logger.LogInformation(
                "Client permission granted for ledger upload. ClientId: {ClientId}, LedgerId: {LedgerId}, WalletId: {WalletId}",
                clientId, ledger.Id, wallet.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error checking client permission for ledger. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);
            return false;
        }
    }

    // Helper methods for the permission checks

    private bool IsWalletActive(Wallet wallet)
    {
        // Define what "active" means for a wallet
        // This could include:
        // 1. Wallet is not locked/frozen
        // 2. Wallet has been created within reasonable timeframe
        // 3. Wallet has positive balance (optional)
        // 4. Wallet has recent activity (optional)

        // For now, we'll implement basic checks:

        // Check 1: Wallet creation date is not too old (optional)
        var maxWalletAgeDays = 365 * 5; // 5 years
        var walletAge = DateTime.UtcNow - wallet.CreatedAt;
        if (walletAge.TotalDays > maxWalletAgeDays)
        {
            logger.LogWarning("Wallet is too old. WalletId: {WalletId}, AgeDays: {AgeDays}",
                wallet.Id, walletAge.TotalDays);
            return false;
        }

        // Check 2: Wallet has recent update (within last 90 days) - optional
        var lastUpdateAge = DateTime.UtcNow - wallet.UpdatedAt;
        if (lastUpdateAge.TotalDays > 90)
        {
            logger.LogWarning("Wallet has not been updated recently. WalletId: {WalletId}, LastUpdateDays: {LastUpdateDays}",
                wallet.Id, lastUpdateAge.TotalDays);
            // Don't return false here - this is just a warning, not a blocker
        }

        // Check 3: Wallet balance is not negative (if that's a business rule)
        if (wallet.Balance.Amount < 0)
        {
            logger.LogWarning("Wallet has negative balance. WalletId: {WalletId}, Balance: {Balance}",
                wallet.Id, wallet.Balance.Amount);
            // Don't return false unless negative balance blocks uploads
        }

        // Add more business rules as needed

        return true;
    }

    private bool LedgerTypeAllowsDocumentAttachments(TransactionType ledgerType)
    {
        // Define which ledger types allow document attachments
        var allowedTypes = new[]
        {
        TransactionType.Deposit,       // Proof of payment for deposits
        TransactionType.Withdrawal,    // Documentation for withdrawals
        TransactionType.Purchase,      // Purchase receipts/invoices
        TransactionType.ServiceFee     // Service fee documentation
        // Note: Adjust based on your business requirements
    };

        var isAllowed = allowedTypes.Contains(ledgerType);

        if (!isAllowed)
        {
            logger.LogDebug("Ledger type {LedgerType} is not in allowed list for attachments", ledgerType);
        }

        return isAllowed;
    }

    private int GetMaxAttachmentsForLedgerType(TransactionType ledgerType)
    {
        // Define maximum number of attachments per ledger type
        return ledgerType switch
        {
            TransactionType.Deposit => 5,      // Multiple proof of payment documents
            TransactionType.Withdrawal => 3,   // Fewer documents for withdrawals
            TransactionType.Purchase => 10,    // Multiple invoices/receipts for purchases
            TransactionType.ServiceFee => 2,   // Minimal documentation for service fees
            _ => 3                             // Default limit
        };
    }

    private async Task<int> GetClientDailyUploadCountAsync(Guid clientId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Query document attachments uploaded by this client today
            // This assumes DocumentAttachment has UploadedBy (string) and UploadedAt properties
            var count = await documentAttachmentRepository.CountClientUploadsTodayAsync(
                clientId.ToString(), today, tomorrow);

            return count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting client daily upload count. ClientId: {ClientId}", clientId);
            return 0; // Fail open - don't block uploads if we can't check the limit
        }
    }

    private int GetDailyUploadLimit()
    {
        // Could be configurable from app settings
        return 20; // Default daily upload limit per client
    }

    // You'll need to add this method to IDocumentRepository and implement it
    public interface IDocumentRepository
    {
        // ... existing methods ...

        Task<int> CountClientUploadsTodayAsync(
            string clientId,
            DateTime todayStart,
            DateTime tomorrowStart,
            CancellationToken cancellationToken = default);

        // ... other methods ...
    }

    private string GetUserFriendlyErrorMessage(string? cloudinaryError)
    {
        if (string.IsNullOrEmpty(cloudinaryError))
            return "Document upload failed due to an unknown error.";

        var error = cloudinaryError.ToLower();

        if (error.Contains("file size") || error.Contains("too large"))
            return "File size exceeds the maximum allowed limit. Please reduce the file size and try again.";

        if (error.Contains("format") || error.Contains("invalid file"))
            return "Unsupported file format. Please use supported image, PDF, or video formats.";

        if (error.Contains("duration") || error.Contains("too long"))
            return "Video duration exceeds the maximum allowed limit. Please use a shorter video.";

        if (error.Contains("corrupt") || error.Contains("damaged"))
            return "The file appears to be corrupted. Please check the file and try again.";

        if (error.Contains("quota") || error.Contains("limit"))
            return "Storage limit reached. Please contact support.";

        if (error.Contains("access denied") || error.Contains("permission"))
            return "Access denied. Please check your permissions or contact support.";

        if (error.Contains("timeout") || error.Contains("connection"))
            return "Upload timed out. Please check your internet connection and try again.";

        // Generic error message for other Cloudinary errors
        return "Document upload failed. Please try again or contact support if the problem persists.";
    }
}