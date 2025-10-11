using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
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
    Task<RepositoryActionResult<Wallet>> RejectDepositAsync(RejectDepositCommand command);
    Task<RepositoryActionResult<ReservedPurchaseDto>> ReservePurchaseAsync(ReservePurchaseCommand command);
    Task<Wallet?> GetByReservationIdAsync(Guid reservationId);
    Task<RepositoryActionResult<Wallet>> ApprovePurchaseAsync(ApprovePurchaseCommand command);
    Task<RepositoryActionResult<Wallet>> CancelPurchaseAsync(CancelPurchaseCommand command);
    Task<Wallet?> GetByClientIdWithDetailsAsync(Guid clientId);
}