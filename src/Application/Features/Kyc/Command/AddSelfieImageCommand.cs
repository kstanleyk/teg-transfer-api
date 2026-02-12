using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Kyc.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Application.Interfaces.Photos;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Kyc.Command;

public record AddSelfieImageCommand(
    Guid ClientId,
    Guid DocumentId,
    IFormFile SelfieImage) : IRequest<Result<Guid>>;

public class AddSelfieImageCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IDocumentService documentService,
    IAppLocalizer localizer,
    ILogger<AddSelfieImageCommandHandler> logger)
    : IRequestHandler<AddSelfieImageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddSelfieImageCommand command, CancellationToken cancellationToken)
    {
        var validator = new AddSelfieImageCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        string? uploadedImagePublicId = null;

        try
        {
            // Get client
            var client = await clientRepository.GetAsync(command.ClientId);
            if (client == null)
                return Result<Guid>.Failed($"Client with ID {command.ClientId} not found.");

            // Get KYC profile with documents
            var kycProfile = await kycProfileRepository.GetKycProfileWithDocumentAsync(command.ClientId, command.DocumentId);
            if (kycProfile == null)
                return Result<Guid>.Failed($"KYC profile not found for client ID {command.ClientId}.");

            // Get the specific document
            var originalDocument = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == command.DocumentId);

            if (originalDocument == null)
                return Result<Guid>.Failed($"Document with ID {command.DocumentId} not found for this client.");

            // Business logic: Check if selfie can be added
            if (originalDocument.IsDeleted)
                return Result<Guid>.Failed("Cannot add selfie to a deleted document.");

            if (originalDocument.Status == KycVerificationStatus.Verified)
                return Result<Guid>.Failed("Cannot add selfie to a verified document. Contact support for assistance.");

            if (originalDocument.Status == KycVerificationStatus.Submitted)
                return Result<Guid>.Failed("Document is submitted for verification. Cannot add selfie at this time.");

            // Check if selfie is supported for this document type
            if (!IsSelfieSupportedForDocumentType(originalDocument.Type))
            {
                return Result<Guid>.Failed($"Selfie images are not supported for {originalDocument.Type} documents.");
            }

            // Upload selfie image to Cloudinary
            var uploadResult = await documentService.UploadDocument(command.SelfieImage);
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                return Result<Guid>.Failed("Failed to upload selfie image. Please try again.");

            uploadedImagePublicId = uploadResult.PublicId;

            // Check for existing selfie document
            var existingSelfieDocument = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Type == KycDocumentType.SelfiePhoto &&
                                   d.DocumentNumber == $"SELFIE_{originalDocument.DocumentNumber}");

            string? oldSelfieImagePublicId;
            Guid selfieDocumentId;

            var parameters = new AddSelfieImageParameters(
                command.ClientId,
                command.DocumentId,
                uploadedImagePublicId);

            if (existingSelfieDocument != null)
            {
                // Update existing selfie document
                oldSelfieImagePublicId = existingSelfieDocument.FrontImagePath;
                selfieDocumentId = existingSelfieDocument.Id;

                var updateResult = await kycProfileRepository.UpdateSelfieImageAsync(parameters);

                if (updateResult.Status == RepositoryActionStatus.Okay || updateResult.Status == RepositoryActionStatus.Updated)
                {
                    // Clean up old selfie image from Cloudinary
                    await TryDeleteOldImage(oldSelfieImagePublicId);

                    var successMessage = localizer["SelfieImageUpdated"];
                    return Result<Guid>.Succeeded(selfieDocumentId, successMessage);
                }
            }
            else
            {
                // Create new selfie document
                var createResult = await kycProfileRepository.CreateSelfieImageAsync(parameters);

                if (createResult.Status == RepositoryActionStatus.Created || createResult.Status == RepositoryActionStatus.Okay)
                {
                    selfieDocumentId = createResult.Entity?.Id ?? Guid.Empty;
                    var successMessage = localizer["SelfieImageAdded"];
                    return Result<Guid>.Succeeded(selfieDocumentId, successMessage);
                }
            }

            // If we get here, repository operation failed
            await TryDeleteUploadedImage(uploadedImagePublicId);

            if (existingSelfieDocument != null)
            {
                return Result<Guid>.Failed("Failed to update selfie image. Please try again.");
            }
            else
            {
                return Result<Guid>.Failed("Failed to add selfie image. Please try again.");
            }
        }
        catch (DomainException ex)
        {
            // Clean up the newly uploaded image
            await TryDeleteUploadedImage(uploadedImagePublicId);
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            // Clean up the newly uploaded image
            await TryDeleteUploadedImage(uploadedImagePublicId);

            logger.LogError(ex, "Error adding selfie image for document {DocumentId} for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result<Guid>.Failed("An error occurred while adding the selfie image. Please try again.");
        }
    }

    private bool IsSelfieSupportedForDocumentType(KycDocumentType documentType)
    {
        // Selfies are typically used for facial verification with identity documents
        return documentType == KycDocumentType.Passport ||
               documentType == KycDocumentType.NationalId ||
               documentType == KycDocumentType.DriversLicense ||
               documentType == KycDocumentType.VotersCard;
    }

    private async Task TryDeleteUploadedImage(string? publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return;

        try
        {
            await documentService.DeleteDocument(publicId);
            logger.LogInformation("Cleaned up uploaded selfie image: {PublicId}", publicId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clean up uploaded selfie image: {PublicId}", publicId);
        }
    }

    private async Task TryDeleteOldImage(string? oldPublicId)
    {
        if (string.IsNullOrEmpty(oldPublicId)) return;

        try
        {
            await documentService.DeleteDocument(oldPublicId);
            logger.LogInformation("Cleaned up old selfie image: {PublicId}", oldPublicId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clean up old selfie image: {PublicId}", oldPublicId);
        }
    }
}

public record AddSelfieImageParameters(
    Guid ClientId,
    Guid DocumentId,
    string SelfieImagePublicId);