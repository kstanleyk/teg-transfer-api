using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class WalletRepository(IDatabaseFactory databaseFactory, ILedgerRepository ledgerRepository)
    : DataRepository<Wallet, Guid>(databaseFactory), IWalletRepository
{
    public async Task<Wallet?> GetByClientIdAsync(Guid clientId) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId);

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

    protected override void DisposeCore()
    {
        ledgerRepository.Dispose();
    }
}