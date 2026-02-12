using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Kyc.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Infrastructure.Persistence.Repository.Kyc;

public class KycProfileRepository(IClientRepository clientRepository,  IDatabaseFactory databaseFactory)
    : DataRepository<KycProfile, Guid>(databaseFactory), IKycProfileRepository
{
    public async Task<RepositoryActionResult<KycProfile>> StartKycProcessAsync(
        StartKycProcessParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the client (already verified in handler, but double-check for safety)
            var client = await clientRepository.GetAsync(parameters.ClientId);
            if (client == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Client with ID {parameters.ClientId} not found."));
            }

            // Check if KYC profile already exists
            var existingProfile = await GetKycProfileByClientIdAsync(parameters.ClientId);
            if (existingProfile != null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile>(
                    existingProfile,
                    RepositoryActionStatus.AlreadyExists,
                    new Exception("KYC profile already exists for this client."));
            }

            // Use the domain factory method to create the KYC profile
            var kycProfile = KycProfile.Create(parameters.ClientId);

            // Initialize email verification with client's email
            kycProfile.InitializeEmailVerification(client.Email);

            // Initialize phone verification with client's phone number
            kycProfile.InitializePhoneVerification(client.PhoneNumber);

            // Add the KYC profile to the context
            DbSet.Add(kycProfile);

            // Also update the client entity to ensure relationship is tracked
            client.GetType().GetProperty("KycProfile")?.SetValue(client, kycProfile);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<KycProfile>(kycProfile, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Check for unique constraint violations
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<KycProfile>(
                    null,
                    RepositoryActionStatus.Error,
                    new Exception("A KYC profile for this client already exists."));
            }

            return new RepositoryActionResult<KycProfile>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<KycProfile?> GetKycProfileByClientIdAsync(Guid clientId)
    {
        return await DbSet
            .Include(k => k.EmailVerification)
            .Include(k => k.PhoneVerification)
            .Include(k => k.IdentityDocuments)
            .FirstOrDefaultAsync(k => k.ClientId == clientId);
    }

    public async Task<KycProfile?> GetKycProfileWithVerificationsAsync(Guid clientId)
    {
        return await DbSet
            .Include(k => k.EmailVerification)
            .Include(k => k.PhoneVerification)
            .Include(k => k.VerificationHistory)
            .FirstOrDefaultAsync(k => k.ClientId == clientId);
    }

    public async Task<KycProfile?> GetKycProfileByClientIdWithEmailVerificationAsync(Guid clientId)
    {
        return await DbSet
            .Include(k => k.EmailVerification!)
            .ThenInclude(e => e.Attempts)
            .FirstOrDefaultAsync(k => k.ClientId == clientId);
    }

    public async Task<KycProfile?> GetKycProfileWithDocumentsAsync(Guid clientId)
    {
        return await DbSet
            .Include(k => k.EmailVerification)
            .Include(k => k.PhoneVerification)
            .Include(k => k.IdentityDocuments)
            .Include(k => k.VerificationHistory)
            .FirstOrDefaultAsync(k => k.ClientId == clientId);
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> UpdateUploadDocumentAsync(
        UploadDocumentParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var kycProfile = await DbSet.FirstOrDefaultAsync(cg => cg.Id == parameters.ClientId);
            if (kycProfile == null)
                return new RepositoryActionResult<IdentityDocument?>(null, RepositoryActionStatus.NotFound);

            // Create domain entity
            var document = kycProfile.AddIdentityDocument(parameters.DocumentType, parameters.DocumentNumber,
                parameters.IssueDate, parameters.ExpiryDate, parameters.FullName, parameters.DateOfBirth,
                parameters.Nationality, parameters.IssuingAuthority);

            // Set image paths (using Cloudinary public IDs)
            document.AddFrontImage(parameters.FrontImagePublic);
            if (!string.IsNullOrEmpty(parameters.BackImagePublicId))
            {
                document.AddBackImage(parameters.BackImagePublicId);
            }

            // Mark as submitted for verification
            document.MarkAsSubmitted();

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(document, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(document, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> UpdateDocumentInfoAsync(
    UpdateDocumentInfoParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Check for duplicate document number (domain validation)
            var duplicateDocument = kycProfile.IdentityDocuments
                .Where(d => d.Id != parameters.DocumentId)
                .FirstOrDefault(d => d.DocumentNumber == parameters.DocumentNumber &&
                                   d.Type == document.Type &&
                                   d.Status != KycVerificationStatus.Expired &&
                                   d.Status != KycVerificationStatus.Rejected);

            if (duplicateDocument != null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException($"Document number {parameters.DocumentNumber} already exists for this document type"));
            }

            // Use domain method to update document info
            // This will throw DomainException if business rules are violated
            document.UpdateDocumentInfo(parameters.DocumentNumber, parameters.IssueDate, parameters.ExpiryDate,
                parameters.FullName, parameters.DateOfBirth, parameters.Nationality, parameters.IssuingAuthority);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Check for unique constraint violations
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new Exception("A document with this number already exists."));
            }

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> UpdateDocumentExpiryAsync(Guid clientId, Guid documentId)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == clientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {clientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == documentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {documentId} not found"));
            }
            document.CheckExpiration();

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.NothingModified);
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Check for unique constraint violations
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new Exception("A document with this number already exists."));
            }

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> AddDocumentFrontImageAsync(
    AddDocumentFrontImageParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with the specific document
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Use domain method to add front image
            // This will throw DomainException if business rules are violated
            document.AddFrontImage(parameters.FrontImagePublicId);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> AddDocumentBackImageAsync(
    AddDocumentBackImageParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with the specific document
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Use domain method to add back image
            // This will throw DomainException if business rules are violated
            document.AddBackImage(parameters.BackImagePublicId);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> CreateSelfieImageAsync(
    AddSelfieImageParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with the specific document
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the original document
            var originalDocument = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (originalDocument == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Check if selfie already exists for this document
            var existingSelfie = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Type == KycDocumentType.SelfiePhoto &&
                                   d.DocumentNumber == $"SELFIE_{originalDocument.DocumentNumber}");

            if (existingSelfie != null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    existingSelfie,
                    RepositoryActionStatus.AlreadyExists,
                    new Exception("Selfie already exists for this document"));
            }

            // Create selfie document using domain method
            var selfieDocument = kycProfile.AddIdentityDocument(
                KycDocumentType.SelfiePhoto,
                $"SELFIE_{originalDocument.DocumentNumber}",
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1), // Selfies typically valid for 1 year
                originalDocument.FullName ?? string.Empty,
                originalDocument.DateOfBirth,
                originalDocument.Nationality);

            // Add the selfie image
            selfieDocument.AddFrontImage(parameters.SelfieImagePublicId);
            selfieDocument.MarkAsSubmitted();

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    selfieDocument,
                    RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> UpdateSelfieImageAsync(
        AddSelfieImageParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the original document
            var originalDocument = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (originalDocument == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Find the existing selfie document
            var selfieDocument = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Type == KycDocumentType.SelfiePhoto &&
                                   d.DocumentNumber == $"SELFIE_{originalDocument.DocumentNumber}");

            if (selfieDocument == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Selfie document not found for document {parameters.DocumentId}"));
            }

            // Use domain method to update selfie image
            selfieDocument.AddFrontImage(parameters.SelfieImagePublicId);

            // Reset verification status if it was previously verified/failed
            if (selfieDocument.Status != KycVerificationStatus.Pending &&
                selfieDocument.Status != KycVerificationStatus.Submitted)
            {
                selfieDocument.ResetVerification();
                selfieDocument.MarkAsSubmitted();
            }

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    selfieDocument,
                    RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<KycProfile?>> SubmitDocumentForVerificationAsync(
        SubmitDocumentForVerificationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.NotFound);
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            //if (document == null)
            //{
            //    await tx.RollbackAsync();
            //    return new RepositoryActionResult(
            //        RepositoryActionStatus.NotFound,
            //        new Exception($"Document with ID {parameters.DocumentId} not found"));
            //}

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.NotFound);
            }

            // Use domain method to submit document for verification
            // This will throw DomainException if business rules are violated
            kycProfile.SubmitIdentityDocumentForVerification(parameters.DocumentId);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<KycProfile?>(kycProfile, RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(kycProfile, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null,  RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<KycProfile?>> SubmitKycForReviewAsync(Guid clientId)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await GetByClientIdAsync(clientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.NotFound);
            }

            kycProfile.SubmitForReview();

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<KycProfile?>(kycProfile, RepositoryActionStatus.Okay);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(kycProfile, RepositoryActionStatus.NothingModified);
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> DeleteDocumentAsync(
        DeleteDocumentParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with the specific document
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Use domain method for soft delete
            // This will throw DomainException if business rules are violated
            document.MarkAsDeleted(parameters.Reason);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    document, // Return the soft-deleted document
                    RepositoryActionStatus.Deleted);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> ApproveDocumentAsync(
    ApproveDocumentParameters parameters,
    CancellationToken cancellationToken = default)
    {
        await using var tx = await Context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId, cancellationToken);

            if (kycProfile == null)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Business logic: Check if document can be approved
            if (document.Status == KycVerificationStatus.Verified)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document is already verified"));
            }

            if (document.Status == KycVerificationStatus.Expired)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Cannot approve an expired document"));
            }

            if (document.Status == KycVerificationStatus.Failed || document.Status == KycVerificationStatus.Rejected)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Cannot approve a failed or rejected document"));
            }

            // Check document expiration
            if (document.ExpiryDate < DateTime.UtcNow)
            {
                document.CheckExpiration();
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document has expired"));
            }

            // Auto-submit if document is pending but has images
            if (document.Status == KycVerificationStatus.Pending && !string.IsNullOrEmpty(document.FrontImagePath))
            {
                document.MarkAsSubmitted();
            }
            else if (document.Status != KycVerificationStatus.Submitted)
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException($"Document must be submitted for verification before approval. Current status: {document.Status}"));
            }

            // Use domain method to approve document (admin verification)
            document.VerifyByAdmin(parameters.ApprovedBy, parameters.Notes);

            var result = await Context.SaveChangesAsync(cancellationToken);
            if (result > 0)
            {
                await tx.CommitAsync(cancellationToken);

                //// Log the approval
                //logger.LogInformation("Document {DocumentId} approved by {ApprovedBy} for client {ClientId}",
                //    parameters.DocumentId, parameters.ApprovedBy, parameters.ClientId);

                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync(cancellationToken);
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);

            //// Log database update errors
            //logger.LogError(ex, "Database error approving document {DocumentId} for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            //logger.LogError(ex, "Unexpected error approving document {DocumentId} for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> RejectDocumentAsync(
        RejectDocumentParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Use domain method to reject document
            // This throws DomainException if business rules are violated
            document.Reject(parameters.RejectedBy, parameters.Reason);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();

                //// Log the rejection
                //logger.LogInformation("Document {DocumentId} rejected by {RejectedBy} for client {ClientId}. Reason: {Reason}",
                //    parameters.DocumentId, parameters.RejectedBy, parameters.ClientId, parameters.Reason);

                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            //logger.LogError(ex, "Database error rejecting document {DocumentId} for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            //logger.LogError(ex, "Unexpected error rejecting document {DocumentId} for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<IdentityDocument?>> MarkDocumentUnderReviewAsync(
    MarkDocumentUnderReviewParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with documents
            var kycProfile = await DbSet
                .Include(k => k.IdentityDocuments)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Get the specific document
            var document = kycProfile.IdentityDocuments
                .FirstOrDefault(d => d.Id == parameters.DocumentId);

            if (document == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"Document with ID {parameters.DocumentId} not found"));
            }

            // Business logic: Check if document can be marked under review
            if (document.Status == KycVerificationStatus.Verified)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document is already verified"));
            }

            if (document.Status == KycVerificationStatus.Expired)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document is expired"));
            }

            if (document.Status == KycVerificationStatus.Rejected || document.Status == KycVerificationStatus.Failed)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException($"Document cannot be marked under review in its current status: {document.Status}"));
            }

            if (document.Status == KycVerificationStatus.Submitted)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document is already submitted/under review"));
            }

            // Check if document has required images
            if (string.IsNullOrEmpty(document.FrontImagePath))
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document cannot be marked under review without a front image"));
            }

            // Check document expiration
            if (document.ExpiryDate < DateTime.UtcNow)
            {
                document.CheckExpiration();
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.Error,
                    new DomainException("Document has expired"));
            }

            // Use domain method to mark as submitted (which is the under review status)
            document.MarkAsSubmitted();

            // Add verification attempt with admin method
            document.MarkAsUnderReview(parameters.ReviewedBy, parameters.Notes);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();

                //// Log the action
                //logger.LogInformation("Document {DocumentId} marked under review by {ReviewedBy} for client {ClientId}",
                //    parameters.DocumentId, parameters.ReviewedBy, parameters.ClientId);

                // Create verification history entry if needed
                try
                {
                    var historyEntry = KycVerificationHistory.Create(
                        kycProfile.Id,
                        KycStatus.Level1Verified, // Previous status
                        KycStatus.Level2Pending,  // New status (if applicable)
                        $"Document {document.Type} marked under review",
                        parameters.ReviewedBy,
                        parameters.Notes);

                    // Assuming VerificationHistory is a collection in KycProfile
                    // You may need to add this property or use a different approach
                    kycProfile.GetType().GetProperty("VerificationHistory")?
                        .GetValue(kycProfile, null)?
                        .GetType()
                        .GetMethod("Add")?
                        .Invoke(null, new object[] { historyEntry });

                    await Context.SaveChangesAsync();
                }
                catch (Exception historyEx)
                {
                    //logger.LogWarning(historyEx, "Failed to add verification history entry");
                    //// Don't fail the main operation
                }

                return new RepositoryActionResult<IdentityDocument?>(
                    document,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IdentityDocument?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            //logger.LogError(ex, "Database error marking document {DocumentId} under review for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            //logger.LogError(ex, "Unexpected error marking document {DocumentId} under review for client {ClientId}",
            //    parameters.DocumentId, parameters.ClientId);

            return new RepositoryActionResult<IdentityDocument?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel1Async(
        ApproveKycLevel1Parameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with all related data
            var kycProfile = await DbSet
                .Include(k => k.EmailVerification)
                .Include(k => k.PhoneVerification)
                .Include(k => k.VerificationHistory)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Use domain method - all business logic is inside the domain entity
            // This will throw DomainException if business rules are violated
            kycProfile.ApproveLevel1(parameters.ApprovedBy, parameters.ExpiresAt, parameters.Notes);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();

                //// Log the approval
                //logger.LogInformation("KYC Level 1 approved for client {ClientId} by {ApprovedBy}. Expires: {ExpiresAt}",
                //    parameters.ClientId, parameters.ApprovedBy, parameters.ExpiresAt);

                return new RepositoryActionResult<KycProfile?>(
                    kycProfile,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            //logger.LogError(ex, "Database error approving KYC Level 1 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            //logger.LogError(ex, "Unexpected error approving KYC Level 1 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel2Async(
    ApproveKycLevel2Parameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with all related data including documents
            var kycProfile = await DbSet
                .Include(k => k.EmailVerification)
                .Include(k => k.PhoneVerification)
                .Include(k => k.IdentityDocuments)
                .Include(k => k.VerificationHistory)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Use domain method - all business logic is inside the domain entity
            // This will throw DomainException if business rules are violated
            kycProfile.ApproveLevel2(parameters.ApprovedBy, parameters.ExpiresAt, parameters.Notes);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();

                //// Log the approval
                //logger.LogInformation("KYC Level 2 approved for client {ClientId} by {ApprovedBy}. Expires: {ExpiresAt}",
                //    parameters.ClientId, parameters.ApprovedBy, parameters.ExpiresAt);

                return new RepositoryActionResult<KycProfile?>(
                    kycProfile,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            //logger.LogError(ex, "Database error approving KYC Level 2 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            //logger.LogError(ex, "Unexpected error approving KYC Level 2 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    public async Task<RepositoryActionResult<KycProfile?>> ApproveKycLevel3Async(
    ApproveKycLevel3Parameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get KYC profile with all related data including documents
            var kycProfile = await DbSet
                .Include(k => k.EmailVerification)
                .Include(k => k.PhoneVerification)
                .Include(k => k.IdentityDocuments)
                .Include(k => k.VerificationHistory)
                .FirstOrDefaultAsync(k => k.ClientId == parameters.ClientId);

            if (kycProfile == null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NotFound,
                    new Exception($"KYC profile not found for client ID {parameters.ClientId}"));
            }

            // Use domain method - all business logic is inside the domain entity
            // This will throw DomainException if business rules are violated
            kycProfile.ApproveLevel3(parameters.ApprovedBy, parameters.ExpiresAt, parameters.Notes);

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();

                // Enhanced logging for Level 3 approval
                var riskScore = CalculateEnhancedRiskScore(kycProfile);
                var approvedDocuments = kycProfile.IdentityDocuments
                    .Where(d => d.IsValid)
                    .Select(d => d.Type.ToString())
                    .ToList();

                //logger.LogInformation(
                //    "KYC Level 3 approved for client {ClientId} by {ApprovedBy}. " +
                //    "Expires: {ExpiresAt}, Risk Score: {RiskScore}, Documents: {Documents}",
                //    parameters.ClientId, parameters.ApprovedBy, parameters.ExpiresAt,
                //    riskScore, string.Join(", ", approvedDocuments));

                return new RepositoryActionResult<KycProfile?>(
                    kycProfile,
                    RepositoryActionStatus.Okay);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<KycProfile?>(
                    null,
                    RepositoryActionStatus.NothingModified);
            }
        }
        catch (DomainException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.ConcurrencyConflict,
                ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            //logger.LogError(ex, "Database error approving KYC Level 3 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            //logger.LogError(ex, "Unexpected error approving KYC Level 3 for client {ClientId}",
            //    parameters.ClientId);

            return new RepositoryActionResult<KycProfile?>(
                null,
                RepositoryActionStatus.Error,
                ex);
        }
    }

    private int CalculateEnhancedRiskScore(KycProfile kycProfile)
    {
        // Simplified risk calculation for logging
        var validDocuments = kycProfile.IdentityDocuments.Count(d => d.IsValid);
        var hasRecentAddressProof = kycProfile.IdentityDocuments.Any(d =>
            d.IsValid && d.Type == KycDocumentType.ProofOfAddress &&
            d.IssueDate >= DateTime.UtcNow.AddMonths(-3));

        var score = 100 - (validDocuments * 10);
        if (hasRecentAddressProof) score -= 20;

        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<KycProfile?> GetByClientIdAsync(Guid clientId)
    {
        return await DbSet
            .Include(k => k.EmailVerification)
            .Include(k => k.PhoneVerification)
            .Include(k => k.IdentityDocuments)
            .Include(k => k.VerificationHistory)
            .FirstOrDefaultAsync(k => k.ClientId == clientId);
    }

    public async Task<bool> ExistsForClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(k => k.ClientId == clientId, cancellationToken);
    }

    public async Task<KycProfile?> GetKycProfileWithDocumentAsync(Guid clientId, Guid documentId)
    {
        return await DbSet
            .Include(k => k.IdentityDocuments.Where(d => !d.IsDeleted)) // Filter out soft-deleted documents
            .FirstOrDefaultAsync(k => k.ClientId == clientId &&
                                      k.IdentityDocuments.Any(d => d.Id == documentId));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // This is a simplified check - you might need to adjust based on your database provider
        var errorMessage = ex.InnerException?.Message ?? ex.Message;
        return errorMessage.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("IX_", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
    }
}