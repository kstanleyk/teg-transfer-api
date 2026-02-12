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

public record SubmitDocumentForVerificationCommand(
    Guid ClientId,
    Guid DocumentId) : IRequest<Result>;

public class SubmitDocumentForVerificationCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<SubmitDocumentForVerificationCommandHandler> logger)
    : IRequestHandler<SubmitDocumentForVerificationCommand, Result>
{
    public async Task<Result> Handle(SubmitDocumentForVerificationCommand command, CancellationToken cancellationToken)
    {
        var validator = new SubmitDocumentForVerificationCommandValidator();
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

            // Business logic: Check if document can be submitted for verification
            if (document.Status != KycVerificationStatus.Pending)
            {
                return document.Status switch
                {
                    KycVerificationStatus.Verified => Result.Failed("Document is already verified."),
                    KycVerificationStatus.Submitted => Result.Failed("Document is already submitted for verification."),
                    KycVerificationStatus.Failed => Result.Failed("Document verification has failed. Please upload a new document."),
                    KycVerificationStatus.Expired => Result.Failed("Document has expired. Please upload a new document."),
                    _ => Result.Failed($"Document cannot be submitted in current status: {document.Status}")
                };
            }

            // Business logic: Check if document has required images
            if (string.IsNullOrEmpty(document.FrontImagePath))
                return Result.Failed("Document front image is required for verification.");

            // Business logic: Check if document is expired
            if (document.ExpiryDate < DateTime.UtcNow)
                return Result.Failed("Document has expired and cannot be submitted for verification.");

            var parameters = new SubmitDocumentForVerificationParameters(
                command.ClientId,
                command.DocumentId);

            var result = await kycProfileRepository.SubmitDocumentForVerificationAsync(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                var successMessage = localizer["DocumentSubmittedForVerification"];
                return Result.Succeeded(successMessage);
            }

            if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("Document not found or already submitted for verification.");
            }

            logger.LogError("Repository returned status {Status} for document submission", result.Status);
            return Result.Failed("An error occurred while submitting the document for verification.");
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting document {DocumentId} for verification for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result.Failed("An error occurred while submitting the document for verification. Please try again.");
        }
    }
}

public record SubmitDocumentForVerificationParameters(
    Guid ClientId,
    Guid DocumentId);

