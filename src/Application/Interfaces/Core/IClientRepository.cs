using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetClientsForExchangeRateQueryAsync();
    Task<Client?> GetClientForExchangeRateQueryAsync(Guid clientId);
}