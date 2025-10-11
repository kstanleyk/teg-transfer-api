using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IWalletRepository : IRepository<Wallet, Guid>
{
    Task<Wallet?> GetByClientIdAsync(Guid clientId);
    Task<RepositoryActionResult<Ledger>> DepositFundsAsync(DepositFundsCommand command);
    Task<RepositoryActionResult<Ledger>> WithdrawFundsAsync(WithdrawFundsCommand command);
    Task<RepositoryActionResult<Wallet>> ApproveDepositAsync(ApproveDepositCommand command);
    Task<Wallet?> GetByClientIdWithPendingLedgersAsync(Guid clientId);
}