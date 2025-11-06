namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record DeactivateClientGroupRequestDto(
    string DeactivatedBy,
    string Reason = "Deactivated");