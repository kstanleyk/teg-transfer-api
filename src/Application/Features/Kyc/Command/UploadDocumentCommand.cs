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

namespace TegWallet.Application.Features.Kyc.Command;

public record UploadDocumentCommand(
    Guid ClientId,
    KycDocumentType DocumentType,
    string DocumentNumber,
    DateTime IssueDate,
    DateTime ExpiryDate,
    IFormFile FrontImage,
    IFormFile? BackImage,
    string? FullName = null,
    DateTime? DateOfBirth = null,
    string? Nationality = null,
    string? IssuingAuthority = null) : IRequest<Result<Guid>>;

public class UploadDocumentCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IDocumentService documentService,
    IAppLocalizer localizer,
    ILogger<UploadDocumentCommandHandler> logger)
    : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        var validator = new UploadDocumentCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        // Get client
        var client = await clientRepository.GetAsync(command.ClientId);
        if (client == null)
            return Result<Guid>.Failed($"Client with ID {command.ClientId} not found.");

        // Get KYC profile
        var kycProfile = await kycProfileRepository.GetKycProfileWithDocumentsAsync(command.ClientId);
        if (kycProfile == null)
            return Result<Guid>.Failed($"KYC profile not found for client ID {command.ClientId}.");

        // Business logic: Check if similar document already exists
        var existingDocument = kycProfile.IdentityDocuments
            .FirstOrDefault(d => d.Type == command.DocumentType &&
                                d.DocumentNumber == command.DocumentNumber &&
                                d.Status != KycVerificationStatus.Expired &&
                                d.Status != KycVerificationStatus.Rejected);

        if (existingDocument != null)
        {
            var message = localizer["DocumentAlreadyExists"];
            return Result<Guid>.Failed(message);
        }

        // Upload documents to Cloudinary
        var frontImageResult = await documentService.UploadDocument(command.FrontImage);
        if (frontImageResult == null || string.IsNullOrEmpty(frontImageResult.PublicId))
            return Result<Guid>.Failed("Failed to upload front image. Please try again.");

        string? backImagePublicId = null;
        if (command.BackImage != null)
        {
            var backImageResult = await documentService.UploadDocument(command.BackImage);
            if (backImageResult == null || string.IsNullOrEmpty(backImageResult.PublicId))
            {
                // If back image fails, delete the front image to avoid orphaned files
                await TryDeleteUploadedFile(frontImageResult.PublicId);
                return Result<Guid>.Failed("Failed to upload back image. Please try again.");
            }
            backImagePublicId = backImageResult.PublicId;
        }

        var uploadDocumentParameters = new UploadDocumentParameters(command.ClientId, command.DocumentType,
            command.DocumentNumber, command.IssueDate, command.ExpiryDate, frontImageResult.PublicId,
            backImagePublicId, command.FullName, command.DateOfBirth, command.Nationality);


        var result = await kycProfileRepository.UpdateUploadDocumentAsync(uploadDocumentParameters);
        if (result.Status != RepositoryActionStatus.Okay)
        {
            await TryDeleteUploadedFile(frontImageResult.PublicId);
            if (!string.IsNullOrEmpty(backImagePublicId))
            {
                await TryDeleteUploadedFile(backImagePublicId);
            }

            return Result<Guid>.Failed("An error occurred while uploading the document. Please try again.");
        }

        var successMessage = localizer["DocumentUploadedSuccess"];
        return Result<Guid>.Succeeded(result.Entity!.Id, successMessage);
    }

    private async Task TryDeleteUploadedFile(string publicId)
    {
        try
        {
            await documentService.DeleteDocument(publicId);
            logger.LogInformation("Cleaned up orphaned file: {PublicId}", publicId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clean up orphaned file: {PublicId}", publicId);
        }
    }
}

public record UploadDocumentParameters(
    Guid ClientId,
    KycDocumentType DocumentType,
    string DocumentNumber,
    DateTime IssueDate,
    DateTime ExpiryDate,
    string FrontImagePublic,
    string? BackImagePublicId,
    string? FullName = null,
    DateTime? DateOfBirth = null,
    string? Nationality = null,
    string? IssuingAuthority = null);

