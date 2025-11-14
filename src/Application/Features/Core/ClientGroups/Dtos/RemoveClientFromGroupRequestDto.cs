namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record RemoveClientFromGroupRequestDto(
    string Reason = "Removed from group");
