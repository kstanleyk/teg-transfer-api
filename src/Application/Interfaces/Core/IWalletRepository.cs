using TegWallet.Application.Features.Core.Wallets.Command;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Model;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IWalletRepository : IRepository<Wallet, Guid>
{
    Task<Wallet?> GetByClientIdAsync(Guid clientId);
    Task<RepositoryActionResult<Ledger>> RequestDepositFundsAsync(RequestDepositFundsCommand command);
    Task<RepositoryActionResult<Ledger>> RequestWithdrawFundsAsync(RequestWithdrawFundsCommand command);
    Task<RepositoryActionResult<Wallet>> ApproveDepositFundsAsync(ApproveDepositFundsCommand fundsCommand);
    Task<Wallet?> GetByClientIdWithPendingLedgersAsync(Guid clientId);
    Task<RepositoryActionResult<Wallet>> RejectDepositAsync(RejectDepositFundsCommand fundsCommand);
    Task<RepositoryActionResult<ReservedPurchaseDto>> ReservePurchaseAsync(ReservePurchaseCommand command);
    Task<Wallet?> GetByReservationIdAsync(Guid reservationId);
    Task<RepositoryActionResult<Wallet>> ApprovePurchaseAsync(ApprovePurchaseCommand command);
    Task<RepositoryActionResult<Wallet>> CancelPurchaseAsync(CancelPurchaseCommand command);
    Task<Wallet?> GetByClientIdWithDetailsAsync(Guid clientId);
    Task<BalanceHistoryData> GetBalanceHistoryDataAsync(Guid walletId, DateTime fromDate, DateTime toDate);
    Task<RepositoryActionResult<Wallet>> ApproveWithdrawFundsAsync(ApproveWithdrawFundsCommand fundsCommand);
    Task<RepositoryActionResult<Wallet>> RejectWithdrawFundsAsync(RejectWithdrawFundsCommand fundsCommand);
    Task<Wallet[]> GetWalletsAsync();
    Task<Wallet[]> GetWalletsForClientsAsync(Guid[] walletIds);
    Task<Wallet?> GetByLedgerIdAsync(Guid ledgerId);
}