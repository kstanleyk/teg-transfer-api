using Dapper;
using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class LedgerRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Ledger, Guid>(databaseFactory), ILedgerRepository
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

    public async Task<List<Ledger>> GetWalletTransactionsByPeriodAsync(Guid walletId, DateTime fromDate,
        DateTime toDate)
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

    public async Task<List<Ledger>> GetLastTenLedgersEachForEveryActiveWalletAsync() => await GetLedgersAsync(GetLastTenLedgersQuery());

    public async Task<List<Ledger>> GetLastTopLedgersForWalletAsync(Guid walletId, int limit = 10) =>
        await DbSet.Where(x => x.WalletId == walletId)
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();

    public async Task<List<Ledger>> GetPendingLedgersAsync() => await GetLedgersAsync(GetPendingLedgersQuery());

    public async Task<List<Ledger>> GetPendingClientLedgersAsync(Guid walletId)
    {
        var parameters = new
        {
            WalletId = walletId
        };
        var ledgers = await Db.QueryAsync<LedgerMapper>(GetPendingClientLedgersQuery(), parameters);

        return ledgers.Select(x => x.ToLedger()).ToList();
    }

    private async Task<List<Ledger>> GetLedgersAsync(string selectSql)
    {
        var ledgers = await Db.QueryAsync<LedgerMapper>(selectSql);

        return ledgers.Select(x => x.ToLedger()).ToList();
    }

    private static string GetLastTenLedgersQuery()
    {
        return """
               WITH ledger_cte(id, walletid, type, amountamount, amountcurrency, status, timestamp, reference, failurereason, description, completiontype, completedat, completedby, reservationid,rownum)
               AS
               (
               SELECT id, wallet_id, type, amount_amount, amount_currency, status, "timestamp", reference, failure_reason, description, completion_type, completed_at, completed_by, reservation_id,
               ROW_NUMBER() OVER (
               PARTITION BY wallet_id
               ORDER BY "timestamp" DESC
               ) AS row_num
               	FROM core.ledger
               )
               SELECT * FROM ledger_cte WHERE rownum <= 10;
               """;
    }

    private static string GetPendingLedgersQuery()
    {
        return """
               WITH ledger_cte(id, walletid, type, amountamount, amountcurrency, status, timestamp, reference, failurereason, description, completiontype, completedat, completedby, reservationid)
                        AS
                        (
                            SELECT id, wallet_id, type, amount_amount, amount_currency, status, "timestamp", reference, failure_reason, description, completion_type, completed_at, completed_by, reservation_id
                            FROM core.ledger WHERE status = 'Pending'
                        )
               SELECT * FROM ledger_cte;
               """;
    }

    private static string GetPendingClientLedgersQuery()
    {
        return """
               WITH ledger_cte(id, walletid, type, amountamount, amountcurrency, status, timestamp, reference, failurereason, description, completiontype, completedat, completedby, reservationid)
                        AS
                        (
                            SELECT id, wallet_id, type, amount_amount, amount_currency, status, "timestamp", reference, failure_reason, description, completion_type, completed_at, completed_by, reservation_id
                            FROM core.ledger WHERE status = 'Pending'
                        )
               SELECT * FROM ledger_cte where walletid = @WalletId;
               """;
    }
}