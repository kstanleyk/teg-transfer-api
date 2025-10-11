using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
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

    public async Task<Wallet?> GetByClientIdWithPendingLedgersAsync(Guid clientId) =>
        await DbSet.AsNoTracking().Include(w => w.LedgerEntries
                .Where(l => l.Type == TransactionType.Deposit &&
                            l.Status == TransactionStatus.Pending))
            .FirstOrDefaultAsync(x => x.ClientId == clientId);

    public async Task<Wallet?> GetByReservationIdAsync(Guid reservationId) =>
        await DbSet
            .FirstOrDefaultAsync(w => w.PurchaseReservations.Any(pr => pr.Id == reservationId));

    public async Task<RepositoryActionResult<Ledger>> DepositFundsAsync(DepositFundsCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Get the wallet
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.NotFound);

            // Step 2: Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);

            var amount = new Money(command.Amount, currency);

            // Step 3: Validate currency matches wallet's base currency
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
            // Step 1: Get the wallet
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Ledger>(null, RepositoryActionStatus.NotFound);

            // Step 2: Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);

            var amount = new Money(command.Amount, currency);

            // Step 3: Validate currency matches wallet's base currency
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
            // Step 1: Get the wallet
            var wallet = await GetByClientIdWithPendingLedgersAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            // Step 2: Get the approved by user (from command or current user service)
            var approvedBy = command.ApprovedBy ?? "System";
            //var approvedBy = "SYSTEM";

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            // Step 3: Process the approval
            wallet.ApproveDeposit(new LedgerId(command.LedgerId), approvedBy);

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
            // Step 1: Get the wallet
            var wallet = await GetByClientIdWithPendingLedgersAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<Wallet>(null, RepositoryActionStatus.NotFound);

            // Step 2: Get the rejected by user (from command or current user service)
            var rejectedBy = command.RejectedBy ?? "System";
            //var approvedBy = "SYSTEM";

            var entry = Context.Entry(wallet);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(wallet);

            // Step 3: Process the rejection
            wallet.RejectDeposit(new LedgerId(command.LedgerId), command.Reason, rejectedBy);

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
            // Step 1: Get the wallet
            var wallet = await GetByClientIdAsync(command.ClientId);
            if (wallet == null)
                return new RepositoryActionResult<ReservedPurchaseDto>(null, RepositoryActionStatus.NotFound);

            // Step 2: Create Money value object
            var currency = Currency.FromCode(command.CurrencyCode);
            var purchaseAmount = new Money(command.PurchaseAmount, currency);
            var serviceFee = new Money(command.ServiceFeeAmount, currency);

            // Step 3: Validate currency matches wallet's base currency
            if (!wallet.HasSufficientBalanceForPurchase(purchaseAmount, serviceFee))
            {
                await tx.RollbackAsync();
                return null;
                //return Result<ReservedPurchaseDto>.Failed(
                //    $"Insufficient balance. Available: {wallet.GetAvailableBalance()} {currency.Code}, " +
                //    $"Required: {purchaseAmount.Amount + serviceFee.Amount} {currency.Code}");
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

    protected override void DisposeCore()
    {
        ledgerRepository.Dispose();
    }
}