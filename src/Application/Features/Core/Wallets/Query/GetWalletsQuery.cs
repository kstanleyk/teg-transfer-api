using MediatR;
using TegWallet.Application.Features.Core.Currencies.Dto;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallets.Query;

public record GetWalletsQuery : IRequest<Result<WalletDto[]>>;

public class GetWalletsQueryHandler(
    IWalletRepository walletRepository)
    : IRequestHandler<GetWalletsQuery, Result<WalletDto[]>>
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<Result<WalletDto[]>> Handle(GetWalletsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var wallets = await _walletRepository.GetWalletsAsync();

            // Ensure we include the necessary related data in the repository method
            // The repository should include Ledgers and Reservations
            var walletDtos = wallets.Select(MapToWalletDto).ToArray();

            return Result<WalletDto[]>.Succeeded(walletDtos);
        }
        catch (Exception ex)
        {
            return Result<WalletDto[]>.Failed($"An error occurred while retrieving wallets: {ex.Message}");
        }
    }

    private static WalletDto MapToWalletDto(Wallet wallet)
    {
        if (wallet == null) return null;

        var dto = new WalletDto
        {
            Id = wallet.Id,
            ClientId = wallet.ClientId,
            Balance = wallet.Balance.Amount,
            AvailableBalance = wallet.AvailableBalance.Amount,
            CurrencyCode = wallet.BaseCurrency.Code,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt,
            Status = DetermineWalletStatus(wallet),
            RecentTransactions = GetRecentTransactions(wallet),
            ActiveReservations = GetActiveReservations(wallet)
        };

        return dto;
    }

    private static CurrencyDto MapToCurrencyDto(Currency currency)
    {
        return new CurrencyDto
        {
            Code = currency.Code,
            Symbol = currency.Symbol,
            DecimalPlaces = currency.DecimalPlaces
        };
    }

    private static List<LedgerDto> GetRecentTransactions(Wallet wallet)
    {
        if (wallet.Ledgers == null || !wallet.Ledgers.Any())
            return new List<LedgerDto>();

        return wallet.Ledgers
            .OrderByDescending(l => l.Timestamp)
            .Take(10)
            .Select(MapToLedgerDto)
            .ToList();
    }

    private static List<ReservationDto> GetActiveReservations(Wallet wallet)
    {
        if (wallet.Reservations == null || !wallet.Reservations.Any())
            return new List<ReservationDto>();

        return wallet.Reservations
            .Where(pr => pr.Status == ReservationStatus.Pending)
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(MapToReservationDto)
            .ToList();
    }

    private static LedgerDto MapToLedgerDto(Ledger ledger)
    {
        var moneyDto = new MoneyDto(ledger.Amount.Amount, ledger.Amount.Currency.Code, ledger.Amount.Currency.Symbol);

        return new LedgerDto
        {
            Id = ledger.Id,
            WalletId = ledger.WalletId,
            Type = ledger.Type.ToString(),
            Amount = moneyDto,
            CurrencyCode = ledger.Amount.Currency.Code,
            Status = ledger.Status.ToString(),
            Timestamp = ledger.Timestamp,
            Reference = ledger.Reference,
            Description = ledger.Description,
            FailureReason = ledger.FailureReason,
            CompletedAt = ledger.CompletedAt,
            CompletedBy = ledger.CompletedBy
        };
    }

    private static ReservationDto MapToReservationDto(Reservation reservation)
    {
        if (reservation == null) return null;

        return new ReservationDto
        {
            Id = reservation.Id,
            ClientId = reservation.ClientId,
            WalletId = reservation.WalletId,
            PurchaseAmount = reservation.PurchaseAmount.Amount,
            ServiceFeeAmount = reservation.ServiceFeeAmount.Amount,
            TotalAmount = reservation.TotalAmount.Amount,
            CurrencyCode = reservation.PurchaseAmount.Currency.Code,
            Description = reservation.Description,
            SupplierDetails = reservation.SupplierDetails,
            PaymentMethod = reservation.PaymentMethod,
            Status = reservation.Status.ToString(),
            CreatedAt = reservation.CreatedAt,
            CompletedAt = reservation.CompletedAt,
            CancelledAt = reservation.CancelledAt,
            CancellationReason = reservation.CancellationReason,
            ProcessedBy = reservation.ProcessedBy
        };
    }

    private static WalletStatus DetermineWalletStatus(Wallet wallet)
    {
        // Exact same logic as in AutoMapper profile
        if (wallet.AvailableBalance.Amount <= 0)
            return WalletStatus.LowBalance;

        // Check if no transactions in last 30 days
        var lastTransactionDate = wallet.Ledgers
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefault()?.Timestamp ?? wallet.CreatedAt;

        if ((DateTime.UtcNow - lastTransactionDate).TotalDays > 30)
            return WalletStatus.NoActivity;

        return WalletStatus.Active;
    }

    // Additional helper methods from AutoMapper profile for completeness
    private static List<BalanceBreakdownDto> GetBalanceBreakdown(Wallet wallet)
    {
        var breakdown = new List<BalanceBreakdownDto>
        {
            new BalanceBreakdownDto
            {
                Type = "Available",
                Amount = wallet.AvailableBalance.Amount,
                Description = "Funds available for immediate use"
            },
            new BalanceBreakdownDto
            {
                Type = "Reserved",
                Amount = CalculateReservedBalance(wallet),
                Description = "Funds reserved for pending purchases"
            },
            new BalanceBreakdownDto
            {
                Type = "Pending Deposits",
                Amount = wallet.Ledgers
                    .Where(l => l.Type == TransactionType.Deposit && l.Status == TransactionStatus.Pending)
                    .Sum(l => l.Amount.Amount),
                Description = "Deposits awaiting approval"
            }
        };

        // Remove zero-value breakdowns (same as AutoMapper)
        return breakdown.Where(b => b.Amount > 0).ToList();
    }

    private static decimal CalculateReservedBalance(Wallet wallet)
    {
        if (wallet.Reservations == null) return 0;

        return wallet.Reservations
            .Where(pr => pr.Status == ReservationStatus.Pending)
            .Sum(pr => pr.TotalAmount.Amount);
    }

    private static BalanceStatus DetermineBalanceStatus(Wallet wallet)
    {
        var availableBalance = wallet.AvailableBalance.Amount;

        if (availableBalance <= 0)
            return BalanceStatus.Insufficient;

        if (availableBalance < 1000) // Same threshold as AutoMapper profile
            return BalanceStatus.LowBalance;

        // Check if no activity in last 30 days
        var lastTransaction = wallet.Ledgers
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefault();

        if (lastTransaction == null || (DateTime.UtcNow - lastTransaction.Timestamp).TotalDays > 30)
            return BalanceStatus.NoActivity;

        return BalanceStatus.Healthy;
    }
}