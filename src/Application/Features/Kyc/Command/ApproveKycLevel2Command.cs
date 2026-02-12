using MediatR;
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

public record ApproveKycLevel2Command(Guid ClientId, string ApprovedBy, DateTime ExpiresAt, string? Notes = null)
    : IRequest<Result>;


public class ApproveKycLevel2CommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<ApproveKycLevel2CommandHandler> logger)
    : IRequestHandler<ApproveKycLevel2Command, Result>
{
    public async Task<Result> Handle(ApproveKycLevel2Command command, CancellationToken cancellationToken)
    {
        var validator = new ApproveKycLevel2CommandValidator();
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

            var parameters = new ApproveKycLevel2Parameters(
                command.ClientId,
                command.ApprovedBy,
                command.ExpiresAt,
                command.Notes);

            var result = await kycProfileRepository.ApproveKycLevel2Async(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                // Update client permissions based on KYC Level 2
                await UpdateClientPermissions(client, cancellationToken);

                // Log KYC approval
                await LogKycApprovalEvent(client, kycProfile, command.ApprovedBy, command.ExpiresAt, cancellationToken);

                // Get document info for response
                var approvedDocuments = GetApprovedDocumentsInfo(kycProfile);

                var successMessage = localizer["KycLevel2Approved"];
                return Result.Succeeded(successMessage);
            }
            else if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("KYC profile not found or cannot be approved.");
            }
            else if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict approving KYC Level 2 for client {ClientId}",
                    command.ClientId);
                return Result.Failed("KYC profile was modified by another user. Please refresh and try again.");
            }
            else if (result.Status == RepositoryActionStatus.Error && result.Exception is DomainException domainEx)
            {
                // Domain exceptions from the domain method
                return Result.Failed(domainEx.Message);
            }
            else
            {
                logger.LogError("Repository returned status {Status} for KYC Level 2 approval", result.Status);
                return Result.Failed("An error occurred while approving KYC Level 2.");
            }
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving KYC Level 2 for client {ClientId}",
                command.ClientId);
            return Result.Failed("An error occurred while approving KYC Level 2. Please try again.");
        }
    }

    private async Task UpdateClientPermissions(Client client, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updated permissions for client {ClientId} after KYC Level 2 approval", client.Id);

            // In real implementation, update client features/permissions for Level 2
            // client.EnableKycLevel2Features();
            // client.UpdateTransactionLimits(TransactionLimits.Level2);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating client permissions after KYC Level 2 approval for client {ClientId}",
                client.Id);
        }
    }

    private async Task LogKycApprovalEvent(Client client, KycProfile kycProfile, string approvedBy,
        DateTime expiresAt, CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = new
            {
                EventType = "KYC_LEVEL2_APPROVED",
                ClientId = client.Id,
                ClientEmail = client.Email,
                KycProfileId = kycProfile.Id,
                ApprovedBy = approvedBy,
                ApprovedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                DocumentCount = kycProfile.IdentityDocuments.Count(d => d.IsValid)
            };

            logger.LogInformation("KYC Level 2 approved: {@AuditLog}", auditLog);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging KYC Level 2 approval event for client {ClientId}", client.Id);
        }
    }

    private List<object> GetApprovedDocumentsInfo(KycProfile kycProfile)
    {
        return kycProfile.IdentityDocuments
            .Where(d => d.IsValid)
            .Select(d => new
            {
                DocumentId = d.Id,
                DocumentType = d.Type,
                DocumentNumber = d.DocumentNumber,
                VerifiedAt = d.VerificationDetails?.VerifiedAt,
                ExpiryDate = d.ExpiryDate
            })
            .ToList<object>();
    }

    private List<string> GetLevel2Permissions()
    {
        return new List<string>
        {
            "Make large purchases",
            "International transfers",
            "Higher transaction limits",
            "Advanced payment methods",
            "Priority support"
        };
    }
}

public record ApproveKycLevel2Parameters(
    Guid ClientId,
    string ApprovedBy,
    DateTime ExpiresAt,
    string? Notes = null);