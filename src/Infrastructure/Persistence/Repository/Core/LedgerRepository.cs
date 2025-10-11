using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class LedgerRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Ledger, LedgerId>(databaseFactory), ILedgerRepository
{
    public async Task<RepositoryActionResult<IEnumerable<Ledger>>> UpdateLedgersAsync(Ledger[] ledgers)
    {
        if (ledgers.Length == 0)
            return new RepositoryActionResult<IEnumerable<Ledger>>(ledgers, RepositoryActionStatus.Okay);

        var result = await AddManyAsync(ledgers);
        if (result.Status == RepositoryActionStatus.Created)
            return new RepositoryActionResult<IEnumerable<Ledger>>(result.Entity, RepositoryActionStatus.Okay);

        return result;
    }
}