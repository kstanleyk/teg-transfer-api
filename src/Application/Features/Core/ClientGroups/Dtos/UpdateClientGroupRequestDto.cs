namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record UpdateClientGroupRequestDto(
    string Name,
    string Description,
    string UpdatedBy);