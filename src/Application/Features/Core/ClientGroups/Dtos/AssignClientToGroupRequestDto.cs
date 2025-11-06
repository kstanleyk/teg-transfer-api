namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record AssignClientToGroupRequestDto(
    Guid ClientGroupId,
    string Reason = "Group assignment");