using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface ILedgerRepository : IRepository<Ledger, Guid>
{
    public Task<RepositoryActionResult<IEnumerable<Ledger>>> UpdateLedgersAsync(Ledger[] ledgers);
    Task<List<Ledger>> GetWalletTransactionsByPeriodAsync(Guid walletId, DateTime fromDate, DateTime toDate);
    Task<decimal> GetWalletBalanceAtDateAsync(Guid walletId, DateTime date);
    Task<List<Ledger>> GetLastTenLedgersEachForEveryActiveWalletAsync();
    Task<List<Ledger>> GetPendingLedgersAsync();
    Task<List<Ledger>> GetLastTopLedgersForWalletAsync(Guid walletId, int limit = 10);
    Task<List<Ledger>> GetPendingClientLedgersAsync(Guid walletId);
}