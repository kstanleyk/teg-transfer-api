using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

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

    public async Task<List<Ledger>> GetWalletTransactionsByPeriodAsync(Guid walletId, DateTime fromDate, DateTime toDate)
    {
        var transactions = await DbSet
            .Where(l => l.WalletId == walletId &&
                        l.Timestamp >= fromDate &&
                        l.Timestamp <= toDate &&
                        (l.Status == TransactionStatus.Completed || l.Status == TransactionStatus.Pending))
            .OrderBy(l => l.Timestamp)
            .ToListAsync();

        return transactions;
    }

    public async Task<decimal> GetWalletBalanceAtDateAsync(Guid walletId, DateTime date)
    {
        try
        {
            // Get all completed transactions up to the specified date
            var transactionsUpToDate = await DbSet
                .Where(l => l.WalletId == walletId &&
                            l.Timestamp <= date &&
                            l.Status == TransactionStatus.Completed)
                .ToListAsync();

            // Calculate net balance by summing deposits and subtracting other transactions
            var netBalance = transactionsUpToDate.Sum(t =>
                t.Type == TransactionType.Deposit ? t.Amount.Amount : -t.Amount.Amount);

            return netBalance;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calculating wallet balance at date {date} for wallet {walletId}", ex);
        }
    }
}