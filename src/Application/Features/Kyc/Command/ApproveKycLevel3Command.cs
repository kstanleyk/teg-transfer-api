using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Kyc.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Kyc.Command;

public record ApproveKycLevel3Command(Guid ClientId, string ApprovedBy, DateTime ExpiresAt, string? Notes = null)
    : IRequest<Result>;

public class ApproveKycLevel3CommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<ApproveKycLevel3CommandHandler> logger)
    : IRequestHandler<ApproveKycLevel3Command, Result>
{
    public async Task<Result> Handle(ApproveKycLevel3Command command, CancellationToken cancellationToken)
    {
        var validator = new ApproveKycLevel3CommandValidator();
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

            // Get KYC profile with all verification details and documents
            var kycProfile = await kycProfileRepository.GetKycProfileWithDocumentsAsync(command.ClientId);
            if (kycProfile == null)
                return Result.Failed($"KYC profile not found for client ID {command.ClientId}.");

            var parameters = new ApproveKycLevel3Parameters(
                command.ClientId,
                command.ApprovedBy,
                command.ExpiresAt,
                command.Notes);

            var result = await kycProfileRepository.ApproveKycLevel3Async(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                // Update client permissions based on KYC Level 3
                await UpdateClientPermissions(client, cancellationToken);

                // Log KYC approval
                await LogKycApprovalEvent(client, kycProfile, command.ApprovedBy, command.ExpiresAt, cancellationToken);

                // Get enhanced verification info for response
                var enhancedVerificationInfo = GetEnhancedVerificationInfo(kycProfile);

                var successMessage = localizer["KycLevel3Approved"];
                return Result.Succeeded(successMessage);
            }

            if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("KYC profile not found or cannot be approved.");
            }

            if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict approving KYC Level 3 for client {ClientId}",
                    command.ClientId);
                return Result.Failed("KYC profile was modified by another user. Please refresh and try again.");
            }

            if (result.Status == RepositoryActionStatus.Error && result.Exception is DomainException domainEx)
            {
                // Domain exceptions from the domain method
                return Result.Failed(domainEx.Message);
            }

            //if (result.Status == RepositoryActionStatus.ValidationFailed)
            //{
            //    return Result.Failed(result.ErrorMessage ?? "Validation failed for KYC Level 3 approval.");
            //}

            logger.LogError("Repository returned status {Status} for KYC Level 3 approval", result.Status);
            return Result.Failed("An error occurred while approving KYC Level 3.");
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving KYC Level 3 for client {ClientId}",
                command.ClientId);
            return Result.Failed("An error occurred while approving KYC Level 3. Please try again.");
        }
    }

    private async Task UpdateClientPermissions(Client client, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updated permissions for client {ClientId} after KYC Level 3 approval", client.Id);

            // In real implementation, update client features/permissions for Level 3
            // client.EnableKycLevel3Features();
            // client.UpdateTransactionLimits(TransactionLimits.Unlimited);
            // client.EnableAdvancedFeatures();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating client permissions after KYC Level 3 approval for client {ClientId}",
                client.Id);
        }
    }

    private async Task LogKycApprovalEvent(Client client, KycProfile kycProfile, string approvedBy,
        DateTime expiresAt, CancellationToken cancellationToken)
    {
        try
        {
            var riskScore = CalculateClientRiskScore(kycProfile);
            var enhancedDocuments = GetEnhancedDocumentsInfo(kycProfile);

            var auditLog = new
            {
                EventType = "KYC_LEVEL3_APPROVED",
                ClientId = client.Id,
                ClientEmail = client.Email,
                KycProfileId = kycProfile.Id,
                ApprovedBy = approvedBy,
                ApprovedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                RiskScore = riskScore,
                EnhancedDocuments = enhancedDocuments,
                TotalDocuments = kycProfile.IdentityDocuments.Count(d => d.IsValid)
            };

            logger.LogInformation("KYC Level 3 approved: {@AuditLog}", auditLog);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging KYC Level 3 approval event for client {ClientId}", client.Id);
        }
    }

    private object GetEnhancedVerificationInfo(KycProfile kycProfile)
    {
        var addressProof = kycProfile.IdentityDocuments
            .FirstOrDefault(d => d.IsValid && d.Type == KycDocumentType.ProofOfAddress);

        var selfieDocument = kycProfile.IdentityDocuments
            .FirstOrDefault(d => d.IsValid && d.Type == KycDocumentType.SelfiePhoto);

        return new
        {
            HasAddressProof = addressProof != null,
            HasSelfieVerification = selfieDocument != null,
            AddressProofAgeDays = addressProof != null ?
                (int)(DateTime.UtcNow - addressProof.IssueDate).TotalDays : 0,
            SelfieVerifiedAt = selfieDocument?.VerificationDetails?.VerifiedAt,
            RiskScore = CalculateClientRiskScore(kycProfile),
            DocumentTypes = kycProfile.IdentityDocuments
                .Where(d => d.IsValid)
                .Select(d => d.Type.ToString())
                .Distinct()
                .ToList()
        };
    }

    private List<object> GetEnhancedDocumentsInfo(KycProfile kycProfile)
    {
        return kycProfile.IdentityDocuments
            .Where(d => d.IsValid &&
                       (d.Type == KycDocumentType.ProofOfAddress ||
                        d.Type == KycDocumentType.SelfiePhoto ||
                        d.Type == KycDocumentType.Passport))
            .Select(d => new
            {
                DocumentId = d.Id,
                DocumentType = d.Type,
                DocumentNumber = d.DocumentNumber,
                IssueDate = d.IssueDate,
                ExpiryDate = d.ExpiryDate,
                VerifiedAt = d.VerificationDetails?.VerifiedAt,
                IsRecent = d.IssueDate >= DateTime.UtcNow.AddMonths(-3)
            })
            .ToList<object>();
    }

    private int CalculateClientRiskScore(KycProfile kycProfile)
    {
        // Simplified risk calculation for logging
        var validDocuments = kycProfile.IdentityDocuments.Count(d => d.IsValid);
        var hasAddressProof = kycProfile.IdentityDocuments.Any(d =>
            d.IsValid && d.Type == KycDocumentType.ProofOfAddress);
        var hasSelfie = kycProfile.IdentityDocuments.Any(d =>
            d.IsValid && d.Type == KycDocumentType.SelfiePhoto);

        var score = 0;

        // Document count penalty
        if (validDocuments < 3) score += 20;

        // Missing enhanced documents penalty
        if (!hasAddressProof) score += 30;
        if (!hasSelfie) score += 30;

        return Math.Min(100, score);
    }

    private List<string> GetLevel3Permissions()
    {
        return new List<string>
        {
            "Unlimited transaction amounts",
            "International business transfers",
            "Priority processing",
            "Dedicated account manager",
            "Advanced reporting",
            "API access",
            "Bulk payment processing",
            "Custom settlement options"
        };
    }
}

public record ApproveKycLevel3Parameters(
    Guid ClientId,
    string ApprovedBy,
    DateTime ExpiresAt,
    string? Notes = null);