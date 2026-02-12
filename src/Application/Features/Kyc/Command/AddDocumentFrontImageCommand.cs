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

namespace TegWallet.Application.Features.Kyc.Command
{
    public record AddDocumentFrontImageCommand(
        Guid ClientId,
        Guid DocumentId,
        IFormFile FrontImage) : IRequest<Result>;

    public class AddDocumentFrontImageCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IDocumentService documentService,
    IAppLocalizer localizer,
    ILogger<AddDocumentFrontImageCommandHandler> logger)
    : IRequestHandler<AddDocumentFrontImageCommand, Result>
    {
        public async Task<Result> Handle(AddDocumentFrontImageCommand command, CancellationToken cancellationToken)
        {
            var validator = new AddDocumentFrontImageCommandValidator();
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
                    return Result.Failed($"Client with ID {command.ClientId} not found.");

                // Get KYC profile with documents
                var kycProfile = await kycProfileRepository.GetKycProfileWithDocumentAsync(command.ClientId, command.DocumentId);
                if (kycProfile == null)
                    return Result.Failed($"KYC profile not found for client ID {command.ClientId}.");

                // Get the specific document
                var document = kycProfile.IdentityDocuments
                    .FirstOrDefault(d => d.Id == command.DocumentId);

                if (document == null)
                    return Result.Failed($"Document with ID {command.DocumentId} not found for this client.");

                // Business logic: Check if document can have front image added/updated
                if (document.IsDeleted)
                    return Result.Failed("Cannot add image to a deleted document.");

                if (document.Status == KycVerificationStatus.Verified)
                    return Result.Failed("Cannot modify image of a verified document. Contact support for assistance.");

                if (document.Status == KycVerificationStatus.Submitted)
                    return Result.Failed("Document is submitted for verification. Cannot modify images at this time.");

                // Upload new front image to Cloudinary
                var uploadResult = await documentService.UploadDocument(command.FrontImage);
                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                    return Result.Failed("Failed to upload front image. Please try again.");

                uploadedImagePublicId = uploadResult.PublicId;

                // Get the old front image public ID for cleanup
                var oldFrontImagePublicId = document.FrontImagePath;

                var parameters = new AddDocumentFrontImageParameters(
                    command.ClientId,
                    command.DocumentId,
                    uploadedImagePublicId);

                var result = await kycProfileRepository.AddDocumentFrontImageAsync(parameters);

                if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
                {
                    // Clean up old front image from Cloudinary
                    await TryDeleteOldImage(oldFrontImagePublicId);

                    var successMessage = localizer["DocumentFrontImageAdded"];
                    return Result.Succeeded(successMessage);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    // Clean up the newly uploaded image since operation failed
                    await TryDeleteUploadedImage(uploadedImagePublicId);
                    return Result.Failed("Document not found or cannot be updated.");
                }
                else if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
                {
                    logger.LogWarning("Concurrency conflict adding front image to document {DocumentId} for client {ClientId}",
                        command.DocumentId, command.ClientId);

                    // Clean up the newly uploaded image
                    await TryDeleteUploadedImage(uploadedImagePublicId);
                    return Result.Failed("Document was modified by another user. Please refresh and try again.");
                }
                else
                {
                    logger.LogError("Repository returned status {Status} for adding document front image", result.Status);

                    // Clean up the newly uploaded image
                    await TryDeleteUploadedImage(uploadedImagePublicId);
                    return Result.Failed("An error occurred while adding the front image.");
                }
            }
            catch (DomainException ex)
            {
                // Clean up the newly uploaded image
                await TryDeleteUploadedImage(uploadedImagePublicId);
                return Result.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                // Clean up the newly uploaded image
                await TryDeleteUploadedImage(uploadedImagePublicId);

                logger.LogError(ex, "Error adding front image to document {DocumentId} for client {ClientId}",
                    command.DocumentId, command.ClientId);
                return Result.Failed("An error occurred while adding the front image. Please try again.");
            }
        }

        private async Task TryDeleteUploadedImage(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return;

            try
            {
                await documentService.DeleteDocument(publicId);
                logger.LogInformation("Cleaned up uploaded front image: {PublicId}", publicId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up uploaded front image: {PublicId}", publicId);
            }
        }

        private async Task TryDeleteOldImage(string? oldPublicId)
        {
            if (string.IsNullOrEmpty(oldPublicId)) return;

            try
            {
                await documentService.DeleteDocument(oldPublicId);
                logger.LogInformation("Cleaned up old front image: {PublicId}", oldPublicId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up old front image: {PublicId}", oldPublicId);
            }
        }
    }

    public record AddDocumentFrontImageParameters(
        Guid ClientId,
        Guid DocumentId,
        string FrontImagePublicId);
}
