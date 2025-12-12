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

public record AttachDocumentToReservationCommand(
    Guid ClientId,
    Guid ReservationId,
    IFormFile File,
    string DocumentType,
    string? Description = null) : IRequest<Result<DocumentUploadResultDto>>;

public class AttachDocumentToReservationCommandHandler
        : IRequestHandler<AttachDocumentToReservationCommand, Result<DocumentUploadResultDto>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IDocumentService _documentService;
    private readonly IDocumentAttachmentRepository _documentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IAppLocalizer _localizer;
    private readonly ILogger<AttachDocumentToReservationCommandHandler> _logger;

    public AttachDocumentToReservationCommandHandler(
        IReservationRepository reservationRepository,
        IDocumentService documentService,
        IDocumentAttachmentRepository documentRepository,
        IClientRepository clientRepository,
        IWalletRepository walletRepository,
        IAppLocalizer localizer,
        ILogger<AttachDocumentToReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _documentService = documentService;
        _documentRepository = documentRepository;
        _clientRepository = clientRepository;
        _walletRepository = walletRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<DocumentUploadResultDto>> Handle(
        AttachDocumentToReservationCommand command,
        CancellationToken cancellationToken)
    {
        // Validate command using the validator
        var validator = new AttachDocumentToReservationCommandValidator();
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
            _logger.LogInformation(
                "Starting document attachment to reservation. ReservationId: {ReservationId}, ClientId: {ClientId}, DocumentType: {DocumentType}, FileName: {FileName}",
                command.ReservationId, command.ClientId, command.DocumentType, command.File.FileName);

            // 1. Validate client exists
            var client = await _clientRepository.GetAsync(command.ClientId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for reservation document upload. ClientId: {ClientId}", command.ClientId);
                return Result<DocumentUploadResultDto>.Failed("Client not found");
            }

            // 2. Validate client is active
            if (client.Status != ClientStatus.Active)
            {
                _logger.LogWarning("Client is not active. ClientId: {ClientId}, Status: {Status}",
                    command.ClientId, client.Status);
                return Result<DocumentUploadResultDto>.Failed("Client account is not active");
            }

            // 3. Validate reservation exists
            var reservation = await _reservationRepository.GetAsync(command.ReservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found. ReservationId: {ReservationId}", command.ReservationId);
                return Result<DocumentUploadResultDto>.Failed("Reservation not found");
            }

            // 4. Check if reservation is pending
            if (!reservation.IsPending)
            {
                _logger.LogWarning(
                    "Cannot attach document to non-pending reservation. ReservationId: {ReservationId}, Status: {Status}",
                    command.ReservationId, reservation.Status);
                return Result<DocumentUploadResultDto>.Failed("Cannot attach document to a reservation that is not in pending status");
            }

            // 5. Check if client has permission to upload to this reservation
            var hasPermission = await ClientHasPermissionToUploadAsync(command.ClientId, reservation);
            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Client does not have permission to upload to reservation. ClientId: {ClientId}, ReservationId: {ReservationId}",
                    command.ClientId, command.ReservationId);
                return Result<DocumentUploadResultDto>.Failed("Client does not have permission to upload documents for this reservation");
            }

            // 6. Upload to Cloudinary
            UploadResult? uploadResult;
            try
            {
                _logger.LogDebug("Uploading file to Cloudinary for reservation. FileName: {FileName}, Size: {Size} bytes, ContentType: {ContentType}",
                    command.File.FileName, command.File.Length, command.File.ContentType);

                uploadResult = await _documentService.UploadDocument(command.File);

                if (uploadResult == null)
                {
                    _logger.LogError("Cloudinary upload returned null result for reservation file: {FileName}", command.File.FileName);
                    return Result<DocumentUploadResultDto>.Failed("Failed to upload document to cloud storage");
                }

                _logger.LogDebug("Cloudinary upload completed for reservation. PublicId: {PublicId}, StatusCode: {StatusCode}, Format: {Format}",
                    uploadResult.PublicId, uploadResult.StatusCode, uploadResult.Format);
            }
            catch (Exception uploadEx)
            {
                _logger.LogError(uploadEx,
                    "Cloudinary upload failed for reservation. ReservationId: {ReservationId}, ClientId: {ClientId}, FileName: {FileName}",
                    command.ReservationId, command.ClientId, command.File.FileName);

                return Result<DocumentUploadResultDto>.Failed(
                    "Failed to upload document to cloud storage. Please try again or contact support.");
            }

            // 7. Validate Cloudinary upload result
            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogWarning(
                    "Cloudinary upload returned non-OK status for reservation. StatusCode: {StatusCode}, Error: {ErrorMessage}",
                    uploadResult.StatusCode, uploadResult.Error?.Message);

                var errorMessage = GetUserFriendlyErrorMessage(uploadResult.Error?.Message);
                return Result<DocumentUploadResultDto>.Failed(errorMessage);
            }

            // 8. Check for specific Cloudinary errors
            if (!string.IsNullOrEmpty(uploadResult.Error?.Message))
            {
                var cloudinaryError = uploadResult.Error.Message.ToLower();
                var userMessage = GetUserFriendlyErrorMessage(cloudinaryError);

                _logger.LogWarning("Cloudinary error for reservation upload: {Error}, UserMessage: {UserMessage}",
                    cloudinaryError, userMessage);

                return Result<DocumentUploadResultDto>.Failed(userMessage);
            }

            // 9. Validate that we got a secure URL
            if (string.IsNullOrEmpty(uploadResult.SecureUrl?.AbsoluteUri))
            {
                _logger.LogError("Cloudinary upload succeeded but returned no secure URL for reservation. PublicId: {PublicId}",
                    uploadResult.PublicId);
                return Result<DocumentUploadResultDto>.Failed("Document upload completed but failed to get file URL");
            }

            // 10. Create document attachment entity (DOMAIN VALIDATION HAPPENS HERE)
            Domain.Entity.Core.DocumentAttachment attachment;
            try
            {
                attachment = Domain.Entity.Core.DocumentAttachment.Create(
                    entityId: command.ReservationId,
                    entityType: nameof(Reservation),
                    fileName: command.File.FileName,
                    fileUrl: uploadResult.SecureUrl.AbsoluteUri,
                    publicId: uploadResult.PublicId,
                    contentType: command.File.ContentType,
                    fileSize: command.File.Length,
                    documentType: command.DocumentType,
                    description: command.Description ?? string.Empty,
                    uploadedBy: command.ClientId.ToString());

                _logger.LogDebug("Reservation document attachment entity created. AttachmentId: {AttachmentId}, FileCategory: {FileCategory}",
                    attachment.Id, attachment.FileCategory);
            }
            catch (DomainException domainEx)
            {
                _logger.LogError(domainEx, "Domain validation failed while creating reservation attachment");
                return Result<DocumentUploadResultDto>.Failed(domainEx.Message);
            }

            // 11. Save to database through repository
            Domain.Entity.Core.DocumentAttachment savedAttachment;
            try
            {
                savedAttachment = await _documentRepository.AddReservationAttachmentAsync(
                    command.ReservationId, attachment, cancellationToken);

                _logger.LogDebug("Reservation document attachment saved to database. AttachmentId: {AttachmentId}",
                    savedAttachment.Id);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx,
                    "Failed to save reservation document attachment to database. ReservationId: {ReservationId}, PublicId: {PublicId}",
                    command.ReservationId, uploadResult.PublicId);

                // Attempt to delete from Cloudinary since database save failed
                try
                {
                    await _documentService.DeleteDocument(uploadResult.PublicId);
                    _logger.LogInformation("Cleaned up Cloudinary file after database save failure for reservation. PublicId: {PublicId}",
                        uploadResult.PublicId);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx,
                        "Failed to clean up Cloudinary file after database save failure for reservation. PublicId: {PublicId}",
                        uploadResult.PublicId);
                }

                return Result<DocumentUploadResultDto>.Failed(
                    "Failed to save document metadata. Please try again.");
            }

            // 12. Update reservation with attachment
            try
            {
                reservation.AddExistingAttachment(savedAttachment);
                //await _reservationRepository.UpdateAsync(reservation, cancellationToken);

                _logger.LogDebug("Reservation updated with new attachment. ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                    command.ReservationId, savedAttachment.Id);
            }
            catch (Exception updateEx)
            {
                // Log but don't fail the operation since the attachment is already saved
                _logger.LogWarning(updateEx,
                    "Failed to update reservation with attachment reference. ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                    command.ReservationId, savedAttachment.Id);
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
            _logger.LogInformation(
                "Document attached to reservation successfully. ReservationId: {ReservationId}, ClientId: {ClientId}, " +
                "AttachmentId: {AttachmentId}, FileType: {ContentType}, FileSize: {FileSize} bytes",
                command.ReservationId, command.ClientId, savedAttachment.Id,
                savedAttachment.ContentType, savedAttachment.FileSize);

            var message = _localizer["DocumentUploadedSuccess"];
            return Result<DocumentUploadResultDto>.Succeeded(resultDto, message);
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain exception in AttachDocumentToReservationCommandHandler");
            return Result<DocumentUploadResultDto>.Failed(ex.Message);
        }
        catch (ValidationException ex)
        {
            // This should have been caught earlier, but just in case
            _logger.LogWarning(ex, "Validation exception in AttachDocumentToReservationCommandHandler");
            return Result<DocumentUploadResultDto>.Failed(string.Join(", ", ex.ValidationErrors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in AttachDocumentToReservationCommandHandler. ReservationId: {ReservationId}, ClientId: {ClientId}",
                command.ReservationId, command.ClientId);

            return Result<DocumentUploadResultDto>.Failed(
                "An unexpected error occurred while attaching the document. Please try again.");
        }
    }

    private async Task<bool> ClientHasPermissionToUploadAsync(
        Guid clientId,
        Reservation reservation)
    {
        try
        {
            _logger.LogDebug(
                "Checking client permission for reservation upload. ClientId: {ClientId}, ReservationId: {ReservationId}, Status: {Status}",
                clientId, reservation.Id, reservation.Status);

            // 1. Check if the client owns this reservation
            if (reservation.ClientId != clientId)
            {
                _logger.LogWarning(
                    "Client does not own this reservation. ClientId: {ClientId}, ReservationClientId: {ReservationClientId}",
                    clientId, reservation.ClientId);
                return false;
            }

            _logger.LogDebug("Client {ClientId} owns reservation {ReservationId}", clientId, reservation.Id);

            // 2. Get client to check account status
            var client = await _clientRepository.GetAsync(clientId);
            if (client == null)
            {
                _logger.LogWarning("Client not found during reservation permission check. ClientId: {ClientId}", clientId);
                return false;
            }

            // 3. Check if client's account is not suspended
            if (client.Status == ClientStatus.Suspended)
            {
                _logger.LogWarning(
                    "Client account is suspended. ClientId: {ClientId}, Status: {Status}",
                    clientId, client.Status);
                return false;
            }

            // 4. Check if client's account is active (not inactive)
            if (client.Status == ClientStatus.Inactive)
            {
                _logger.LogWarning(
                    "Client account is inactive. ClientId: {ClientId}, Status: {Status}",
                    clientId, client.Status);
                return false;
            }

            _logger.LogDebug("Client {ClientId} account status is valid: {Status}", clientId, client.Status);

            // 5. Verify the reservation is still valid
            if (!IsReservationValid(reservation))
            {
                _logger.LogWarning("Reservation is not valid. ReservationId: {ReservationId}", reservation.Id);
                return false;
            }

            _logger.LogDebug("Reservation {ReservationId} is valid", reservation.Id);

            // 6. Check if reservation allows document attachments (some types might not)
            if (!ReservationAllowsDocumentAttachments(reservation))
            {
                _logger.LogWarning(
                    "Reservation type does not allow document attachments. ReservationId: {ReservationId}",
                    reservation.Id);
                return false;
            }

            _logger.LogDebug("Reservation {ReservationId} allows document attachments", reservation.Id);

            // 7. Check if maximum attachment limit hasn't been reached for this reservation
            var attachmentCount = await _documentRepository.GetReservationAttachmentCountAsync(
                reservation.Id);

            var maxAttachments = GetMaxAttachmentsForReservation();

            if (attachmentCount >= maxAttachments)
            {
                _logger.LogWarning(
                    "Document attachment limit exceeded for reservation. ReservationId: {ReservationId}, Current: {Current}, Max: {Max}",
                    reservation.Id, attachmentCount, maxAttachments);
                return false;
            }

            _logger.LogDebug("Attachment count within limits. ReservationId: {ReservationId}, Count: {Count}, Max: {Max}",
                reservation.Id, attachmentCount, maxAttachments);

            // 8. Check reservation amount is valid
            if (!IsReservationAmountValid(reservation))
            {
                _logger.LogWarning(
                    "Reservation amount is invalid. ReservationId: {ReservationId}, TotalAmount: {TotalAmount}",
                    reservation.Id, reservation.TotalAmount.Amount);
                return false;
            }

            _logger.LogDebug("Reservation amount is valid: {Amount}", reservation.TotalAmount.Amount);

            // 9. Check if reservation was created within a reasonable timeframe
            if (!IsReservationCreationTimeValid(reservation))
            {
                _logger.LogWarning(
                    "Reservation creation time is invalid. ReservationId: {ReservationId}, CreatedAt: {CreatedAt}",
                    reservation.Id, reservation.CreatedAt);
                return false;
            }

            _logger.LogDebug("Reservation creation time is valid: {CreatedAt}", reservation.CreatedAt);

            // 10. Check if client has exceeded daily upload limit for reservations
            var todayUploads = await GetClientDailyReservationUploadCountAsync(clientId);
            var dailyReservationLimit = GetDailyReservationUploadLimit();

            if (todayUploads >= dailyReservationLimit)
            {
                _logger.LogWarning(
                    "Client daily reservation upload limit exceeded. ClientId: {ClientId}, Today: {Today}, Limit: {Limit}",
                    clientId, todayUploads, dailyReservationLimit);
                return false;
            }

            _logger.LogDebug("Client daily reservation upload count: {Today}/{Limit}", todayUploads, dailyReservationLimit);

            // 11. Check if reservation payment method requires documentation
            if (RequiresDocumentationForPaymentMethod(reservation.PaymentMethod))
            {
                _logger.LogDebug("Payment method {PaymentMethod} requires documentation", reservation.PaymentMethod);
            }

            // 12. Check if wallet associated with reservation is active
            var wallet = await _walletRepository.GetAsync(reservation.WalletId);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for reservation. ReservationId: {ReservationId}, WalletId: {WalletId}",
                    reservation.Id, reservation.WalletId);
                return false;
            }

            if (!IsWalletActiveForReservation(wallet, reservation))
            {
                _logger.LogWarning("Wallet is not active for reservation. WalletId: {WalletId}, ReservationId: {ReservationId}",
                    wallet.Id, reservation.Id);
                return false;
            }

            _logger.LogDebug("Wallet {WalletId} is active for reservation", wallet.Id);

            // All checks passed
            _logger.LogInformation(
                "Client permission granted for reservation upload. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking client permission for reservation. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);
            return false;
        }
    }

    // Helper methods for reservation permission checks

    private bool IsReservationValid(Reservation reservation)
    {
        // Check various aspects of reservation validity

        // 1. Reservation must be pending (already checked in handler, but double-check)
        if (!reservation.IsPending)
        {
            _logger.LogDebug("Reservation is not pending. Status: {Status}", reservation.Status);
            return false;
        }

        // 2. Reservation should not be too old (e.g., created within last 30 days)
        var maxReservationAgeDays = 30;
        var reservationAge = DateTime.UtcNow - reservation.CreatedAt;

        if (reservationAge.TotalDays > maxReservationAgeDays)
        {
            _logger.LogDebug("Reservation is too old. AgeDays: {AgeDays}", reservationAge.TotalDays);
            return false;
        }

        // 3. Check if reservation has valid purchase and service fee ledgers
        // (Assuming these are required fields that should already be validated)

        // 4. Check if reservation description is not empty
        if (string.IsNullOrWhiteSpace(reservation.Description))
        {
            _logger.LogDebug("Reservation description is empty");
            return false;
        }

        // 5. Check if supplier details are provided
        if (string.IsNullOrWhiteSpace(reservation.SupplierDetails))
        {
            _logger.LogDebug("Reservation supplier details are empty");
            return false;
        }

        // 6. Check if payment method is valid
        if (string.IsNullOrWhiteSpace(reservation.PaymentMethod))
        {
            _logger.LogDebug("Reservation payment method is empty");
            return false;
        }

        return true;
    }

    private bool IsReservationAmountValid(Reservation reservation)
    {
        // Validate reservation amounts

        // 1. Total amount should be positive
        if (reservation.TotalAmount.Amount <= 0)
        {
            _logger.LogDebug("Reservation total amount is not positive: {Amount}", reservation.TotalAmount.Amount);
            return false;
        }

        // 2. Purchase amount should be positive
        if (reservation.PurchaseAmount.Amount <= 0)
        {
            _logger.LogDebug("Reservation purchase amount is not positive: {Amount}", reservation.PurchaseAmount.Amount);
            return false;
        }

        // 3. Service fee should not be negative
        if (reservation.ServiceFeeAmount.Amount < 0)
        {
            _logger.LogDebug("Reservation service fee is negative: {Amount}", reservation.ServiceFeeAmount.Amount);
            return false;
        }

        // 4. Total should equal purchase + service fee (with tolerance for rounding)
        var expectedTotal = reservation.PurchaseAmount.Amount + reservation.ServiceFeeAmount.Amount;
        var tolerance = 0.01m; // 1 cent tolerance for rounding

        if (Math.Abs(reservation.TotalAmount.Amount - expectedTotal) > tolerance)
        {
            _logger.LogDebug("Reservation total amount mismatch. Expected: {Expected}, Actual: {Actual}",
                expectedTotal, reservation.TotalAmount.Amount);
            return false;
        }

        // 5. Check for reasonable maximum amount (anti-fraud measure)
        var maxReservationAmount = 1000000m; // 1 million max
        if (reservation.TotalAmount.Amount > maxReservationAmount)
        {
            _logger.LogDebug("Reservation amount exceeds maximum: {Amount} > {Max}",
                reservation.TotalAmount.Amount, maxReservationAmount);
            return false;
        }

        return true;
    }

    private bool ReservationAllowsDocumentAttachments(Reservation reservation)
    {
        // Define which reservation characteristics allow document attachments
        // This could be based on:
        // - Reservation type (if you have different types)
        // - Payment method
        // - Amount thresholds
        // - Supplier type

        // For now, all reservations allow attachments, but you can add business rules:

        // Example: Only reservations with certain payment methods allow attachments
        var disallowedPaymentMethods = new[]
        {
            "INTERNAL_TRANSFER",  // Internal transfers might not need documentation
            "BONUS",              // Bonus payments might not need documentation
            "REFUND"              // Refunds might not need documentation
        };

        if (disallowedPaymentMethods.Contains(reservation.PaymentMethod.ToUpper()))
        {
            _logger.LogDebug("Payment method {PaymentMethod} disallows attachments", reservation.PaymentMethod);
            return false;
        }

        // Example: Minimum amount threshold for requiring/allowin documentation
        var minAmountForDocumentation = 1000m; // Only reservations above 1000 require docs
        if (reservation.TotalAmount.Amount < minAmountForDocumentation)
        {
            _logger.LogDebug("Reservation amount {Amount} below documentation threshold {Threshold}",
                reservation.TotalAmount.Amount, minAmountForDocumentation);
            // Don't return false - small amounts can still have documentation
        }

        return true;
    }

    private int GetMaxAttachmentsForReservation()
    {
        // Could be configurable from app settings
        // Different limits based on reservation amount or type
        return 15; // Default maximum attachments per reservation
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

    private bool IsReservationCreationTimeValid(Reservation reservation)
    {
        // Check if reservation was created within reasonable timeframe

        // 1. Reservation should not be created in the future
        if (reservation.CreatedAt > DateTime.UtcNow.AddMinutes(5)) // 5 minute tolerance for clock skew
        {
            _logger.LogDebug("Reservation creation time is in the future: {CreatedAt}", reservation.CreatedAt);
            return false;
        }

        // 2. Reservation should not be too old (already checked in IsReservationValid)
        // but we can have a shorter window for document uploads
        var maxUploadWindowHours = 72; // 3 days to upload documents after reservation creation

        if (reservation.CreatedAt < DateTime.UtcNow.AddHours(-maxUploadWindowHours))
        {
            _logger.LogDebug("Reservation is too old for document upload. CreatedAt: {CreatedAt}, MaxWindowHours: {MaxWindowHours}",
                reservation.CreatedAt, maxUploadWindowHours);
            return false;
        }

        return true;
    }

    private async Task<int> GetClientDailyReservationUploadCountAsync(Guid clientId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Query reservation document attachments uploaded by this client today
            // This requires joining DocumentAttachments with Reservations
            var count = await _documentRepository.CountClientReservationUploadsTodayAsync(
                clientId.ToString(), today, tomorrow);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client daily reservation upload count. ClientId: {ClientId}", clientId);
            return 0; // Fail open - don't block uploads if we can't check the limit
        }
    }

    private int GetDailyReservationUploadLimit()
    {
        // Could be configurable from app settings
        // Might be different from general document upload limit
        return 50; // Default daily reservation upload limit per client
    }

    private bool RequiresDocumentationForPaymentMethod(string paymentMethod)
    {
        // Define which payment methods require mandatory documentation
        var paymentMethodsRequiringDocumentation = new[]
        {
        "BANK_TRANSFER",
        "WIRE_TRANSFER",
        "CHECK",
        "CASH_DEPOSIT",
        "THIRD_PARTY_PAYMENT"
    };

        var requiresDocs = paymentMethodsRequiringDocumentation.Contains(paymentMethod.ToUpper());

        if (requiresDocs)
        {
            _logger.LogDebug("Payment method {PaymentMethod} requires documentation", paymentMethod);
        }

        return requiresDocs;
    }

    private bool IsWalletActiveForReservation(Wallet wallet, Reservation reservation)
    {
        // Check if wallet is suitable for the reservation

        // 1. Wallet should have sufficient available balance for the reservation
        // (This is already checked when reservation is created, but verify)
        if (wallet.AvailableBalance.Amount < reservation.TotalAmount.Amount)
        {
            _logger.LogDebug("Wallet has insufficient available balance. Available: {Available}, Required: {Required}",
                wallet.AvailableBalance.Amount, reservation.TotalAmount.Amount);
            // Don't block uploads - reservation might have been created when balance was sufficient
        }

        // 2. Wallet currency should match reservation currency
        if (wallet.BaseCurrency != reservation.TotalAmount.Currency)
        {
            _logger.LogDebug("Wallet currency mismatch. Wallet: {WalletCurrency}, Reservation: {ReservationCurrency}",
                wallet.BaseCurrency.Code, reservation.TotalAmount.Currency.Code);
            // This should ideally not happen, but log it
        }

        // 3. Wallet should not be locked/frozen
        // (Add a WalletStatus property if you have one)

        return true;
    }
}