using TegWallet.Domain.Entity.Kyc;

namespace TegWallet.Application.Features.Kyc.Dto;

public record StartKycProcessResponse(Guid KycProfileId, KycStatus Status, DateTime CreatedAt)
{
    public static StartKycProcessResponse FromDomain(KycProfile profile) =>
        new(profile.Id, profile.Status, profile.CreatedAt);
}