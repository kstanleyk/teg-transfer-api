using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Ledgers.Query;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallets.Query;

public record GetWalletByClientIdQuery(Guid ClientId) : IRequest<Result<WalletDto>>;

public class GetWalletByClientIdQueryHandler(
    IWalletRepository walletRepository, IMapper mapper)
    : GetLedgersQueryHandlerBase(walletRepository, mapper), IRequestHandler<GetWalletByClientIdQuery, Result<WalletDto>>
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<Result<WalletDto>> Handle(GetWalletByClientIdQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdWithDetailsAsync(query.ClientId);
        if(wallet == null)
            return Result<WalletDto>.Failed($"Wallet for ClientId {query.ClientId} not found.");

        var walletMapper = new WalletMapper();
        var walletDto = walletMapper.MapToDto(wallet);

        return Result<WalletDto>.Succeeded(walletDto);
    }
}

public class WalletMapper
{
    public WalletDto MapToDto(Wallet wallet)
    {
        return new WalletDto
        {
            Id = wallet.Id,
            ClientId = wallet.ClientId,
            Balance = wallet.Balance.Amount,
            AvailableBalance = wallet.AvailableBalance.Amount,
            CurrencyCode = wallet.BaseCurrency.Code,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt,
            Status = DetermineWalletStatus(wallet),
            RecentTransactions = MapRecentTransactions(wallet),
            ActiveReservations = MapActiveReservations(wallet)
        };
    }

    private WalletStatus DetermineWalletStatus(Wallet wallet)
    {
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

    private List<LedgerDto> MapRecentTransactions(Wallet wallet)
    {
        return wallet.Ledgers
            .OrderByDescending(l => l.Timestamp)
            .Take(10)
            .Select(x=> MapLedgerToDto(x, wallet.ClientId))
            .ToList();
    }

    private LedgerDto MapLedgerToDto(Ledger ledger, Guid clientId)
    {
        return new LedgerDto
        {
            Id = ledger.Id,
            WalletId = ledger.WalletId,
            ClientId = clientId,
            Type = ledger.Type.ToString(),
            Status = ledger.Status.ToString(),
            Amount = MapMoneyToDto(ledger.Amount),
            CurrencyCode = ledger.Amount.Currency.Code,
            Description = ledger.Description,
            Reference = ledger.Reference,
            Timestamp = ledger.Timestamp,
            CompletedAt = ledger.CompletedAt,
            CompletedBy = ledger.CompletedBy,
            FailureReason = ledger.FailureReason
        };
    }

    private List<ReservationDto> MapActiveReservations(Wallet wallet)
    {
        return wallet.Reservations
            .Where(pr => pr.Status == ReservationStatus.Pending)
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(MapReservationToDto)
            .ToList();
    }

    private ReservationDto MapReservationToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            ClientId = reservation.ClientId,
            WalletId = reservation.WalletId,
            PurchaseAmount = reservation.PurchaseAmount.Amount,
            ServiceFeeAmount = reservation.ServiceFeeAmount.Amount,
            TotalAmount = reservation.TotalAmount.Amount,
            CurrencyCode = reservation.PurchaseAmount.Currency.Code,
            Status = reservation.Status.ToString(),
            Description = reservation.Description,
            SupplierDetails = reservation.SupplierDetails,
            PaymentMethod = reservation.PaymentMethod,
            PurchaseLedgerId = reservation.PurchaseLedgerId,
            ServiceFeeLedgerId = reservation.ServiceFeeLedgerId,
            CreatedAt = reservation.CreatedAt,
            CompletedAt = reservation.CompletedAt,
            CancelledAt = reservation.CancelledAt,
            //CancelledBy = reservation.CancelledBy,
            //CancelledReason = reservation.CancelledReason,
            //CompletedBy = reservation.CompletedBy,
            //Metadata = reservation.Metadata
        };
    }

    public MoneyDto MapMoneyToDto(Money money)
    {
        if (money == null)
            throw new ArgumentNullException(nameof(money));

        if (money.Currency == null)
            throw new ArgumentException("Money.Currency cannot be null");

        return new MoneyDto
        {
            Amount = money.Amount,
            CurrencyCode = money.Currency.Code,
            CurrencySymbol = money.Currency.Symbol
        };
    }

}