using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface ILedgerRepository : IRepository<Ledger, LedgerId>
{
    public Task<RepositoryActionResult<IEnumerable<Ledger>>> UpdateLedgersAsync(Ledger[] ledgers);
}