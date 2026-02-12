using MediatR;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Application.Features.Kyc.Command;

public record SubmitKycForReviewCommand(Guid ClientId, string SubmittedBy, string? Notes = null)
    : IRequest<Result<SubmitKycForReviewResponse>>;


public class SubmitKycForReviewCommandHandler(
    IKycProfileRepository kycProfileRepository,
    IClientRepository clientRepository)
    : IRequestHandler<SubmitKycForReviewCommand, Result<SubmitKycForReviewResponse>>
{
    public async Task<Result<SubmitKycForReviewResponse>> Handle(SubmitKycForReviewCommand request,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetAsync(request.ClientId);
        if (client == null)
            return Result<SubmitKycForReviewResponse>.Failed(
                $"Client not found: {request.ClientId}");

        if (client.Status != ClientStatus.Active)
            return Result<SubmitKycForReviewResponse>.Failed(
                $"Client is not active. Current status: {client.Status}");

        var kycResult = await kycProfileRepository.SubmitKycForReviewAsync(request.ClientId);
        if (kycResult.Status != RepositoryActionStatus.Okay)
            return Result<SubmitKycForReviewResponse>.Failed(
                "KYC submission submission failed. please try again.");

        var kycProfile = kycResult.Entity;

        return Result<SubmitKycForReviewResponse>.Succeeded(new SubmitKycForReviewResponse(
            kycProfile!.Id, kycProfile.Status, DateTime.UtcNow));
    }
}

public record SubmitKycForReviewResponse(
    Guid KycProfileId,
    KycStatus NewStatus,
    DateTime SubmittedAt);