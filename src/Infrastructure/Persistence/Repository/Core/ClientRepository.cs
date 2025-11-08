using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Infrastructure.Persistence.Context;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ClientRepository(TegWalletContext context) : Disposable, IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetClientsForExchangeRateQueryAsync()
    {
        return await context.ClientSet
            .Include(c => c.Wallet)
            .Include(c => c.ClientGroup)
            .Where(c => c.Status == ClientStatus.Active)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<Client?> GetClientForExchangeRateQueryAsync(Guid clientId)
    {
        return await context.ClientSet
            .Include(c => c.Wallet)
            .Include(c => c.ClientGroup)
            .Where(c => c.Status == ClientStatus.Active)
            .FirstOrDefaultAsync(x => x.Id == clientId);
    }

    protected override void DisposeCore()
    {
        context.Dispose();
    }
}