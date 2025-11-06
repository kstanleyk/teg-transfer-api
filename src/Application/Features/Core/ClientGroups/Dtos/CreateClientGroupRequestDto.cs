namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record CreateClientGroupRequestDto(
    string Name,
    string Description,
    string CreatedBy);