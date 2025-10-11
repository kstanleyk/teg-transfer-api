using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Model;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class WalletRepository(IDatabaseFactory databaseFactory, ILedgerRepository ledgerRepository)
    : DataRepository<Wallet, Guid>(databaseFactory), IWalletRepository
{
    public async Task<Wallet?> GetByClientIdAsync(Guid clientId) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId);

    public async Task<Wallet?> GetByClientIdWithDetailsAsync(Guid clientId)
    {
        var wallet = await DbSet
            .Include(w => w.LedgerEntries
                .OrderByDescending(l => l.Timestamp)
                .Take(10)) // Last 10 transactions
            .Include(w => w.PurchaseReservations
                .Where(pr => pr.Status == PurchaseReservationStatus.Pending)
                .OrderByDescending(pr => pr.CreatedAt))
            .AsSplitQuery() // For better performance with multiple includes
            .FirstOrDefaultAsync(w => w.ClientId == clientId);

        return wallet;
    }

    public async Task<Wallet?> GetByClientIdWithPendingLedgersAsync(Guid clientId) =>
        await DbSet.AsNoTracking().Include(w => w.LedgerEntries
                .Where(l => l.Type == TransactionType.Deposit &&
                            l.Status == TransactionStatus.Pending))
            .FirstOrDefaultAsync(x => x.ClientId == clientId);

    public async Task<Wallet?> GetByReservationIdAsync(Guid reservationId) =>
        await DbSet
            .Include(x=>x.PurchaseReservations)
            .Include(x=>x.LedgerEntries)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.PurchaseReservations.Any(pr => pr.Id == reservationId));

    public async Task<RepositoryActionResult<Ledger>> DepositFundsAsync(DepositFundsCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.NotFound);

            // Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);
            var amount = new Money(command.Amount, currency);

            // Validate currency matches wallet's base currency
            if (amount.Currency != wallet.BaseCurrency)
            {
                await tx.RollbackAsync();
                throw new InvalidOperationException($"Deposit currency ({amount.Currency.Code}) must match wallet's base currency ({wallet.BaseCurrency.Code})");
            }

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            var ledger = wallet.Deposit(amount, command.Reference, command.Description);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Ledger>(ledger, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Ledger>> WithdrawFundsAsync(WithdrawFundsCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the wallet
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.NotFound);

            // Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);

            var amount = new Money(command.Amount, currency);

            // Validate currency matches wallet's base currency
            if (amount.Currency != wallet.BaseCurrency)
            {
                await tx.RollbackAsync();
                throw new InvalidOperationException($"Deposit currency ({amount.Currency.Code}) must match wallet's base currency ({wallet.BaseCurrency.Code})");
            }

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            var ledger = wallet.Withdraw(amount, command.Description);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Ledger>(ledger, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Wallet>> ApproveDepositAsync(ApproveDepositCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the wallet
            var wallet = await GetByClientIdWithPendingLedgersAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            // Process the approval
            wallet.ApproveDeposit(new LedgerId(command.LedgerId), command.ApprovedBy);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Wallet>(wallet, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Wallet>> RejectDepositAsync(RejectDepositCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the wallet
            var wallet = await GetByClientIdWithPendingLedgersAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            // Process the rejection
            wallet.RejectDeposit(new LedgerId(command.LedgerId), command.Reason, command.RejectedBy);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Wallet>(wallet, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ReservedPurchaseDto>> ReservePurchaseAsync(ReservePurchaseCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the wallet
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<ReservedPurchaseDto>(null, RepositoryActionStatus.NotFound);

            // Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);
            var purchaseAmount = new Money(command.PurchaseAmount, currency);
            var serviceFee = new Money(command.ServiceFeeAmount, currency);

            // Validate currency matches wallet's base currency
            if (!wallet.HasSufficientBalanceForPurchase(purchaseAmount, serviceFee))
            {
                await tx.RollbackAsync();
                throw new InvalidOperationException(
                    $"Insufficient balance. Available: {wallet.GetAvailableBalance()} {currency.Code}, " +
                    $"Required: {purchaseAmount.Amount + serviceFee.Amount} {currency.Code}");
            }

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            var (reservation, purchaseLedger, serviceFeeLedger) = wallet.ReserveForPurchase(
                purchaseAmount,
                serviceFee,
                command.Description,
                command.SupplierDetails,
                command.PaymentMethod);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            var dto = new ReservedPurchaseDto
            {
                ReservationId = reservation.Id,
                PurchaseLedgerId = purchaseLedger.Id.Value,
                ServiceFeeLedgerId = serviceFeeLedger.Id.Value,
                PurchaseAmount = purchaseAmount.Amount,
                ServiceFeeAmount = serviceFee.Amount,
                TotalAmount = purchaseAmount.Amount + serviceFee.Amount,
                CurrencyCode = currency.Code,
                Description = reservation.Description,
                SupplierDetails = reservation.SupplierDetails,
                PaymentMethod = reservation.PaymentMethod,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt
            };

            await tx.CommitAsync();
            return new RepositoryActionResult<ReservedPurchaseDto>(dto, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ReservedPurchaseDto>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ReservedPurchaseDto>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ReservedPurchaseDto>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Wallet>> ApprovePurchaseAsync(ApprovePurchaseCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var wallet = await GetByReservationIdAsync(command.ReservationId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            wallet.CompletePurchase(command.ReservationId, command.ProcessedBy);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Wallet>(wallet, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Wallet>> CancelPurchaseAsync(CancelPurchaseCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var wallet = await GetByReservationIdAsync(command.ReservationId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            wallet.CancelPurchase(command.ReservationId, command.Reason, command.CancelledBy);

            var result = await SaveChangesAsync();
            if (result > 0) await entry.ReloadAsync();

            await tx.CommitAsync();
            return new RepositoryActionResult<Wallet>(wallet, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<BalanceHistoryData> GetBalanceHistoryDataAsync(Guid walletId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var wallet = await DbSet
                .Include(w => w.LedgerEntries)
                .FirstOrDefaultAsync(w => w.Id == walletId);

            if (wallet == null)
                throw new InvalidOperationException($"Wallet not found: {walletId}");

            // Get transactions within the period
            var transactions = await ledgerRepository.GetWalletTransactionsByPeriodAsync(walletId, fromDate, toDate);

            // Calculate starting balance (balance before the period)
            var startingBalance = await ledgerRepository.GetWalletBalanceAtDateAsync(walletId, fromDate.AddDays(-1));

            // Calculate daily balances
            var dailyBalances = CalculateDailyBalances(transactions, startingBalance, fromDate, toDate);

            return new BalanceHistoryData
            {
                Transactions = transactions,
                StartingBalance = startingBalance,
                DailyBalances = dailyBalances
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving balance history data for wallet {walletId}", ex);
        }
    }

    private static List<DailyBalance> CalculateDailyBalances(List<Ledger> transactions, decimal startingBalance,
        DateTime fromDate, DateTime toDate)
    {
        var dailyBalances = new List<DailyBalance>();
        var currentBalance = startingBalance;
        var currentDate = fromDate.Date;

        // Group transactions by date
        var transactionsByDate = transactions
            .Where(t => t.Timestamp.Date >= fromDate.Date && t.Timestamp.Date <= toDate.Date)
            .GroupBy(t => t.Timestamp.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        while (currentDate <= toDate.Date)
        {
            if (transactionsByDate.TryGetValue(currentDate, out var dayTransactions))
            {
                // Calculate net change for the day
                var netChange = dayTransactions.Sum(t =>
                    t.Type == TransactionType.Deposit ? t.Amount.Amount : -t.Amount.Amount);

                currentBalance += netChange;

                dailyBalances.Add(new DailyBalance
                {
                    Date = currentDate,
                    TotalBalance = currentBalance,
                    AvailableBalance = currentBalance, // Simplified - in reality, we'd need to track available balance separately
                    TransactionCount = dayTransactions.Count
                });
            }
            else
            {
                // No transactions on this day, balance remains the same
                dailyBalances.Add(new DailyBalance
                {
                    Date = currentDate,
                    TotalBalance = currentBalance,
                    AvailableBalance = currentBalance,
                    TransactionCount = 0
                });
            }

            currentDate = currentDate.AddDays(1);
        }

        return dailyBalances;
    }

    protected override void DisposeCore()
    {
        ledgerRepository.Dispose();
    }
}