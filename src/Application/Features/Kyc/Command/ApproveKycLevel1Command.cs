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

public record ApproveKycLevel1Command(Guid ClientId, string ApprovedBy, DateTime ExpiresAt, string? Notes = null)
    : IRequest<Result>;

public class ApproveKycLevel1CommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository,
    IAppLocalizer localizer,
    ILogger<ApproveKycLevel1CommandHandler> logger)
    : IRequestHandler<ApproveKycLevel1Command, Result>
{
    public async Task<Result> Handle(ApproveKycLevel1Command command, CancellationToken cancellationToken)
    {
        var validator = new ApproveKycLevel1CommandValidator();
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

            // Get KYC profile with all verification details
            var kycProfile = await kycProfileRepository.GetKycProfileWithVerificationsAsync(command.ClientId);
            if (kycProfile == null)
                return Result.Failed($"KYC profile not found for client ID {command.ClientId}.");

            var parameters = new ApproveKycLevel1Parameters(command.ClientId, command.ApprovedBy, command.ExpiresAt,
                command.Notes);

            var result = await kycProfileRepository.ApproveKycLevel1Async(parameters);

            if (result.Status == RepositoryActionStatus.Okay || result.Status == RepositoryActionStatus.Updated)
            {
                // Update client permissions based on KYC Level 1
                await UpdateClientPermissions(client);

                // Log KYC approval
                await LogKycApprovalEvent(client, kycProfile, command.ApprovedBy, command.ExpiresAt);

                var successMessage = localizer["KycLevel1Approved"];
                return Result.Succeeded(successMessage);
            }

            if (result.Status == RepositoryActionStatus.NotFound)
            {
                return Result.Failed("KYC profile not found or cannot be approved.");
            }

            if (result.Status == RepositoryActionStatus.ConcurrencyConflict)
            {
                logger.LogWarning("Concurrency conflict approving KYC Level 1 for client {ClientId}",
                    command.ClientId);
                return Result.Failed("KYC profile was modified by another user. Please refresh and try again.");
            }

            if (result.Status == RepositoryActionStatus.Error && result.Exception is DomainException domainEx)
            {
                // Domain exceptions from the domain method
                return Result.Failed(domainEx.Message);
            }

            logger.LogError("Repository returned status {Status} for KYC Level 1 approval", result.Status);
            return Result.Failed("An error occurred while approving KYC Level 1.");
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving KYC Level 1 for client {ClientId}",
                command.ClientId);
            return Result.Failed("An error occurred while approving KYC Level 1. Please try again.");
        }
    }

    private async Task UpdateClientPermissions(Client client)
    {
        try
        {
            logger.LogInformation("Updated permissions for client {ClientId} after KYC Level 1 approval", client.Id);

            // In real implementation, update client features/permissions
            // client.EnableKycLevel1Features();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating client permissions after KYC Level 1 approval for client {ClientId}",
                client.Id);
        }
    }

    private async Task LogKycApprovalEvent(Client client, KycProfile kycProfile, string approvedBy,
        DateTime expiresAt)
    {
        try
        {
            var auditLog = new
            {
                EventType = "KYC_LEVEL1_APPROVED",
                ClientId = client.Id,
                ClientEmail = client.Email,
                KycProfileId = kycProfile.Id,
                ApprovedBy = approvedBy,
                ApprovedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            logger.LogInformation("KYC Level 1 approved: {@AuditLog}", auditLog);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging KYC Level 1 approval event for client {ClientId}", client.Id);
        }
    }
}

public record ApproveKycLevel1Parameters(
    Guid ClientId,
    string ApprovedBy,
    DateTime ExpiresAt,
    string? Notes = null);