using MediatR;
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
    public record DeleteDocumentCommand(
        Guid ClientId,
        Guid DocumentId,
        string Reason) : IRequest<Result>;


    public class DeleteDocumentCommandHandler(
        IKycProfileRepository kycProfileRepository,
        IClientRepository clientRepository,
        IDocumentService documentService,
        IAppLocalizer localizer,
        ILogger<DeleteDocumentCommandHandler> logger)
        : IRequestHandler<DeleteDocumentCommand, Result>
    {
        public async Task<Result> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
        {
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

                // Business logic: Check if document can be deleted
                if (document.Status == KycVerificationStatus.Verified)
                    return Result.Failed("Cannot delete a document that has been verified. Contact support for assistance.");

                if (document.Status == KycVerificationStatus.Submitted)
                    return Result.Failed("Cannot delete a document that is submitted for verification. Please wait for verification to complete or contact support.");

                var parameters = new DeleteDocumentParameters(
                    command.ClientId,
                    command.DocumentId,
                    command.Reason);

                var result = await kycProfileRepository.DeleteDocumentAsync(parameters);

                if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Deleted)
                {
                    // Access the deleted document from result.Entity if needed
                    var deletedDocument = result.Entity;

                    // Schedule Cloudinary cleanup for the soft-deleted document
                    if (deletedDocument != null)
                    {
                        ScheduleCloudinaryCleanup(deletedDocument);
                    }

                    var successMessage = localizer["DocumentDeletedSuccess"];
                    return Result.Succeeded(successMessage);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return Result.Failed("Document not found or already deleted.");
                }
                else if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
                {
                    logger.LogWarning("Concurrency conflict deleting document {DocumentId} for client {ClientId}",
                        command.DocumentId, command.ClientId);
                    return Result.Failed("Document was modified by another user. Please refresh and try again.");
                }
                else
                {
                    logger.LogError("Repository returned status {Status} for document deletion", result.Status);
                    return Result.Failed("An error occurred while deleting the document.");
                }
            }
            catch (DomainException ex)
            {
                return Result.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting document {DocumentId} for client {ClientId}",
                    command.DocumentId, command.ClientId);
                return Result.Failed("An error occurred while deleting the document. Please try again.");
            }
        }

        private void ScheduleCloudinaryCleanup(IdentityDocument document)
        {
            // Schedule background job to clean up Cloudinary files
            // This can be done using Hangfire, BackgroundService, or similar
            logger.LogInformation("Scheduled Cloudinary cleanup for soft-deleted document {DocumentId}", document.Id);

            // Example: _backgroundJobClient.Enqueue(() => DeleteFromCloudinaryAsync(document.Id));
        }

        // Background job method
        //public async Task DeleteFromCloudinaryAsync(Guid documentId)
        //{
        //    try
        //    {
        //        // Get document (including soft-deleted ones)
        //        var document = await GetDocumentForCleanup(documentId);
        //        if (document == null) return;

        //        // Delete from Cloudinary
        //        if (!string.IsNullOrEmpty(document.FrontImagePath))
        //            await documentService.DeleteDocument(document.FrontImagePath);

        //        if (!string.IsNullOrEmpty(document.BackImagePath))
        //            await documentService.DeleteDocument(document.BackImagePath);

        //        logger.LogInformation("Cloudinary cleanup completed for soft-deleted document {DocumentId}", documentId);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error in Cloudinary cleanup for soft-deleted document {DocumentId}", documentId);
        //    }
        //}
    }

    public record DeleteDocumentParameters(
        Guid ClientId,
        Guid DocumentId,
        string Reason);
}
