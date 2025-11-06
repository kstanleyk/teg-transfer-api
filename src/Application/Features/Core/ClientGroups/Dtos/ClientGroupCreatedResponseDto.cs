namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record ClientGroupCreatedResponseDto
{
    public Guid ClientGroupId { get; init; }
    public string? Message { get; init; }
}