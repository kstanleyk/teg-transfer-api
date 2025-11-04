using AutoMapper;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Ledgers.Query;

public abstract class GetLedgersQueryHandlerBase(
    IWalletRepository walletRepository,
    IMapper mapper)
    
{
    protected async Task<LedgerDto[]> UpdateClientIds(Guid[] walletIds, List<Ledger> ledgers)
    {
        var wallets = await walletRepository.GetWalletsForClientsAsync(walletIds);

        var walletDict = wallets.ToDictionary(x => x.Id, x => x.ClientId);

        var ledgerWithClientIds = mapper.Map<LedgerDto[]>(ledgers);
        foreach (var ledger in ledgerWithClientIds)
        {
            ledger.ClientId = walletDict[ledger.WalletId];
        }

        return ledgerWithClientIds;
    }
}