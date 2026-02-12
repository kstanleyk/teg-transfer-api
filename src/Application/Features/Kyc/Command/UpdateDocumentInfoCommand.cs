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

public record UpdateDocumentInfoCommand(
    Guid ClientId,
    Guid DocumentId,
    string DocumentNumber,
    DateTime IssueDate,
    DateTime ExpiryDate,
    string? FullName = null,
    DateTime? DateOfBirth = null,
    string? Nationality = null,
    string? IssuingAuthority = null) : IRequest<Result>;

public class UpdateDocumentInfoCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<UpdateDocumentInfoCommandHandler> logger)
    : IRequestHandler<UpdateDocumentInfoCommand, Result>
{
    public async Task<Result> Handle(UpdateDocumentInfoCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateDocumentInfoCommandValidator();
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

            // Business logic: Check if document can be updated
            if (document.Status == KycVerificationStatus.Verified)
                return Result.Failed("Cannot update a document that has already been verified.");

            if (document.Status == KycVerificationStatus.Expired)
                return Result.Failed("Cannot update an expired document. Please upload a new document.");

            if (document.Status == KycVerificationStatus.Failed)
                return Result.Failed("Cannot update a failed document. Please upload a new document.");

            // Business logic: Check if new document number conflicts with another document
            var duplicateDocument = kycProfile.IdentityDocuments
                .Where(d => d.Id != command.DocumentId)
                .FirstOrDefault(d => d.DocumentNumber == command.DocumentNumber &&
                                   d.Type == document.Type &&
                                   d.Status != KycVerificationStatus.Expired &&
                                   d.Status != KycVerificationStatus.Rejected);

            if (duplicateDocument != null)
            {
                var message = localizer["DocumentNumberAlreadyExists"];
                return Result.Failed(message);
            }

            // Business logic: Check if document is already submitted/under review
            if (document.Status == KycVerificationStatus.Submitted)
            {
                logger.LogWarning("Attempted to update document {DocumentId} that is already submitted for verification",
                    command.DocumentId);
                return Result.Failed("Document is already submitted for verification. Cannot update at this time.");
            }

            var parameters = new UpdateDocumentInfoParameters(
                command.ClientId,
                command.DocumentId,
                command.DocumentNumber,
                command.IssueDate,
                command.ExpiryDate,
                command.FullName,
                command.DateOfBirth,
                command.Nationality,
                command.IssuingAuthority);

            var result = await kycProfileRepository.UpdateDocumentInfoAsync(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                var successMessage = localizer["DocumentInfoUpdated"];
                return Result.Succeeded(successMessage);
            }
            else if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("Document not found or cannot be updated.");
            }
            else if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict updating document {DocumentId} for client {ClientId}",
                    command.DocumentId, command.ClientId);
                return Result.Failed("Document was modified by another user. Please refresh and try again.");
            }
            else
            {
                logger.LogError("Repository returned status {Status} for document update", result.Status);
                return Result.Failed("An error occurred while updating the document information.");
            }
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating document {DocumentId} info for client {ClientId}",
                command.DocumentId, command.ClientId);
            return Result.Failed("An error occurred while updating the document information. Please try again.");
        }
    }
}

public record UpdateDocumentInfoParameters(
    Guid ClientId,
    Guid DocumentId,
    string DocumentNumber,
    DateTime IssueDate,
    DateTime ExpiryDate,
    string? FullName = null,
    DateTime? DateOfBirth = null,
    string? Nationality = null,
    string? IssuingAuthority = null);