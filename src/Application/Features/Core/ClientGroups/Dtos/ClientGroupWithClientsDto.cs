namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public class ClientGroupWithClientsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public IReadOnlyList<ClientGroupClientDto> Clients { get; set; } = new List<ClientGroupClientDto>();
    public int TotalClients => Clients.Count;
}