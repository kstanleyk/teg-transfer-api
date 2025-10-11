using AutoMapper;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Model;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallet;

public class WalletProfile : Profile
{
    public WalletProfile()
    {
        CreateMap<DepositRequestDto, DepositFundsCommand>();

        CreateMap<WithdrawalRequestDto, WithdrawFundsCommand>();

        CreateMap<ApproveDepositDto, ApproveDepositCommand>();

        CreateMap<RejectDepositDto, RejectDepositCommand>();

        // Ledger to TransactionDto
        CreateMap<Ledger, TransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.WalletId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason));

        // Money to MoneyDto
        CreateMap<Money, MoneyDto>()
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency.Symbol));

        // Wallet to WalletDto mapping
        CreateMap<Domain.Entity.Core.Wallet, WalletDto>()
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance.Amount))
            .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.AvailableBalance.Amount))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.BaseCurrency.Code))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineWalletStatus(src)))
            .ForMember(dest => dest.RecentTransactions, opt => opt.MapFrom(src =>
                src.LedgerEntries
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)))
            .ForMember(dest => dest.ActiveReservations, opt => opt.MapFrom(src =>
                src.PurchaseReservations
                    .Where(pr => pr.Status == PurchaseReservationStatus.Pending)
                    .OrderByDescending(pr => pr.CreatedAt)));

        // PurchaseReservation to PurchaseReservationDto mapping
        CreateMap<PurchaseReservation, PurchaseReservationDto>()
            .ForMember(dest => dest.PurchaseAmount, opt => opt.MapFrom(src => src.PurchaseAmount.Amount))
            .ForMember(dest => dest.ServiceFeeAmount, opt => opt.MapFrom(src => src.ServiceFeeAmount.Amount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.PurchaseAmount.Currency.Code))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Money value object to simple decimal (for cases where we just need the amount)
        CreateMap<Money, decimal>()
            .ConvertUsing(src => src.Amount);

        // Currency value object to string (for cases where we just need the code)
        CreateMap<Currency, string>()
            .ConvertUsing(src => src.Code);

        CreateMap<Domain.Entity.Core.Wallet, WalletBalanceDto>()
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TotalBalance, opt => opt.MapFrom(src => src.Balance.Amount))
            .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.AvailableBalance.Amount))
            .ForMember(dest => dest.PendingBalance, opt => opt.MapFrom(src => src.GetPendingBalance()))
            .ForMember(dest => dest.ReservedBalance, opt => opt.MapFrom(src => CalculateReservedBalance(src)))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.BaseCurrency.Code))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineBalanceStatus(src)))
            .ForMember(dest => dest.Breakdown, opt => opt.MapFrom(src => GetBalanceBreakdown(src)));

        CreateMap<Domain.Entity.Core.Wallet, SimpleBalanceDto>()
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.AvailableBalance.Amount))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.BaseCurrency.Code));

        // Balance history mappings
        CreateMap<DailyBalance, BalanceSnapshotDto>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.PeriodLabel, opt => opt.MapFrom(src => src.Date.ToString("MMM dd")))
            .ForMember(dest => dest.NetChange, opt => opt.Ignore()) // Calculated in handler
            .ForMember(dest => dest.PendingBalance, opt => opt.Ignore()); // Not available in DailyBalance

        CreateMap<BalanceHistoryData, BalanceHistoryDto>()
            .ForMember(dest => dest.WalletId, opt => opt.Ignore())
            .ForMember(dest => dest.ClientId, opt => opt.Ignore())
            .ForMember(dest => dest.CurrencyCode, opt => opt.Ignore())
            .ForMember(dest => dest.FromDate, opt => opt.Ignore())
            .ForMember(dest => dest.ToDate, opt => opt.Ignore())
            .ForMember(dest => dest.Period, opt => opt.Ignore())
            .ForMember(dest => dest.Snapshots, opt => opt.Ignore())
            .ForMember(dest => dest.Summary, opt => opt.Ignore());
    }

    private static List<BalanceBreakdownDto> GetBalanceBreakdown(Domain.Entity.Core.Wallet wallet)
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
                Amount = wallet.LedgerEntries
                    .Where(l => l.Type == TransactionType.Deposit && l.Status == TransactionStatus.Pending)
                    .Sum(l => l.Amount.Amount),
                Description = "Deposits awaiting approval"
            }
        };

        // Remove zero-value breakdowns
        return breakdown.Where(b => b.Amount > 0).ToList();
    }

    private static decimal CalculateReservedBalance(Domain.Entity.Core.Wallet wallet)
    {
        return wallet.PurchaseReservations
            .Where(pr => pr.Status == PurchaseReservationStatus.Pending)
            .Sum(pr => pr.TotalAmount.Amount);
    }

    private static BalanceStatus DetermineBalanceStatus(Domain.Entity.Core.Wallet wallet)
    {
        var availableBalance = wallet.AvailableBalance.Amount;

        if (availableBalance <= 0)
            return BalanceStatus.Insufficient;

        if (availableBalance < 1000) // Example threshold for low balance
            return BalanceStatus.LowBalance;

        // Check if no activity in last 30 days
        var lastTransaction = wallet.LedgerEntries
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefault();

        if (lastTransaction == null || (DateTime.UtcNow - lastTransaction.Timestamp).TotalDays > 30)
            return BalanceStatus.NoActivity;

        return BalanceStatus.Healthy;
    }

    private static WalletStatus DetermineWalletStatus(Domain.Entity.Core.Wallet wallet)
    {
        // Business logic to determine wallet status
        if (wallet.AvailableBalance.Amount <= 0)
            return WalletStatus.LowBalance;

        // Check if no transactions in last 30 days
        var lastTransactionDate = wallet.LedgerEntries
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefault()?.Timestamp ?? wallet.CreatedAt;

        if ((DateTime.UtcNow - lastTransactionDate).TotalDays > 30)
            return WalletStatus.NoActivity;

        return WalletStatus.Active;
    }
}